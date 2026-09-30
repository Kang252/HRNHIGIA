"""One local, bounded JPEG comparison; no network, persisted images, or liveness claim."""
import base64
import binascii
import hashlib
import json
import math
import os
import pathlib
import sys

# Set before importing cv2, including its native image codec.
os.environ["OPENCV_IO_MAX_IMAGE_PIXELS"] = "12000000"
os.environ["OPENCV_IO_MAX_IMAGE_WIDTH"] = "4096"
os.environ["OPENCV_IO_MAX_IMAGE_HEIGHT"] = "4096"
os.environ["OPENCV_LOG_LEVEL"] = "SILENT"
os.environ["OMP_NUM_THREADS"] = "1"
os.environ["OPENBLAS_NUM_THREADS"] = "1"

MODEL_VERSION = "yunet-2023mar+sface-2021dec@47534e27c985"
MAX_IMAGE_BYTES = 2 * 1024 * 1024
MAX_REQUEST_BYTES = 6 * 1024 * 1024
DEFAULT_THRESHOLD = 0.45
MODEL_FILES = (
    ("face_detection_yunet_2023mar.onnx", 232589,
     "8f2383e4dd3cfbb4553ea8718107fc0423210dc964f9f4280604804ed2552fa4"),
    ("face_recognition_sface_2021dec.onnx", 38696353,
     "0ba9fbfa01b5270c96627c4ef784da859931e02f04419c829e83484087c34e79"),
)


class Rejected(Exception):
    def __init__(self, status, detail):
        self.status, self.detail = status, detail


def jpeg_dimensions(data):
    """Read bounded JPEG SOF metadata before a native decoder allocates pixels."""
    if len(data) < 12 or data[:2] != b"\xff\xd8" or data[-2:] != b"\xff\xd9":
        raise Rejected("INVALID_IMAGE", "JPEG_REQUIRED")
    position = 2
    while position < len(data) - 2:
        if data[position] != 0xFF:
            raise Rejected("INVALID_IMAGE", "JPEG_HEADER")
        while position < len(data) and data[position] == 0xFF:
            position += 1
        if position >= len(data):
            break
        marker = data[position]
        position += 1
        if marker in (0xDA, 0xD9):
            break
        if marker == 0x01 or 0xD0 <= marker <= 0xD7:
            continue
        if position + 2 > len(data):
            break
        size = int.from_bytes(data[position:position + 2], "big")
        if size < 2 or position + size > len(data):
            break
        if marker in (0xC0, 0xC1, 0xC2):
            if size < 8 or data[position + 2] != 8:
                break
            height = int.from_bytes(data[position + 3:position + 5], "big")
            width = int.from_bytes(data[position + 5:position + 7], "big")
            if width < 80 or height < 80 or width > 4096 or height > 4096 or width * height > 12_000_000:
                raise Rejected("INVALID_IMAGE", "IMAGE_DIMENSIONS")
            return width, height
        position += size
    raise Rejected("INVALID_IMAGE", "JPEG_HEADER")


def photo_bytes(value):
    if not isinstance(value, str) or len(value) > ((MAX_IMAGE_BYTES + 2) // 3) * 4:
        raise Rejected("INVALID_IMAGE", "IMAGE_BYTES")
    try:
        data = base64.b64decode(value, validate=True)
    except (binascii.Error, ValueError):
        raise Rejected("INVALID_IMAGE", "BASE64") from None
    if len(data) > MAX_IMAGE_BYTES:
        raise Rejected("INVALID_IMAGE", "IMAGE_BYTES")
    return data, jpeg_dimensions(data)


def checked_models(directory):
    files = []
    for name, size, checksum in MODEL_FILES:
        path = directory / name
        try:
            if path.stat().st_size != size:
                raise Rejected("UNAVAILABLE", "MODEL_INTEGRITY")
            with path.open("rb") as stream:
                actual = hashlib.file_digest(stream, "sha256").hexdigest()
            if actual != checksum:
                raise Rejected("UNAVAILABLE", "MODEL_INTEGRITY")
        except OSError:
            raise Rejected("UNAVAILABLE", "MODEL_MISSING") from None
        files.append(str(path))
    return files


def feature(cv2, np, detector, recognizer, data, dimensions, side):
    image = cv2.imdecode(np.frombuffer(data, dtype=np.uint8), cv2.IMREAD_COLOR | cv2.IMREAD_IGNORE_ORIENTATION)
    if image is None or (image.shape[1], image.shape[0]) != dimensions:
        raise Rejected("INVALID_IMAGE", side + "_DECODE")
    # Bound detection cost, keeping the same scale for landmarks and SFace alignment.
    scale = min(1.0, 960 / max(dimensions))
    if scale < 1:
        image = cv2.resize(image, None, fx=scale, fy=scale, interpolation=cv2.INTER_AREA)
    detector.setInputSize((image.shape[1], image.shape[0]))
    _, faces = detector.detect(image)
    if faces is None or len(faces) == 0:
        raise Rejected("NO_FACE", side + "_NO_FACE")
    if len(faces) != 1:
        raise Rejected("MULTIPLE_FACES", side + "_MULTIPLE_FACES")
    face = faces[0]
    x, y, width, height = face[:4]
    if min(width, height) < 80 or x < 0 or y < 0 or x + width > image.shape[1] or y + height > image.shape[0]:
        raise Rejected("LOW_QUALITY", side + "_FACE_SIZE_OR_CROP")
    crop = image[int(y):int(y + height), int(x):int(x + width)]
    gray = cv2.cvtColor(cv2.resize(crop, (112, 112)), cv2.COLOR_BGR2GRAY)
    # Conservative trial quality gates; neither is a presentation-attack check.
    if float(gray.mean()) < 35 or float(gray.mean()) > 220 or float(cv2.Laplacian(gray, cv2.CV_64F).var()) < 20:
        raise Rejected("LOW_QUALITY", side + "_LIGHT_OR_BLUR")
    aligned = recognizer.alignCrop(image, face)
    return recognizer.feature(aligned)


def compare(request, model_dir, threshold):
    reference, reference_size = photo_bytes(request.get("Reference"))
    candidate, candidate_size = photo_bytes(request.get("Candidate"))
    detector_path, recognizer_path = checked_models(model_dir)
    import cv2
    import numpy as np
    cv2.setNumThreads(1)
    cv2.ocl.setUseOpenCL(False)
    detector = cv2.FaceDetectorYN.create(detector_path, "", (320, 320), 0.9, 0.3, 5000)
    recognizer = cv2.FaceRecognizerSF.create(recognizer_path, "")
    left = feature(cv2, np, detector, recognizer, reference, reference_size, "REFERENCE")
    right = feature(cv2, np, detector, recognizer, candidate, candidate_size, "CANDIDATE")
    score = float(recognizer.match(left, right, cv2.FaceRecognizerSF_FR_COSINE))
    if not math.isfinite(score):
        raise Rejected("UNAVAILABLE", "INVALID_SCORE")
    score = max(-1.0, min(1.0, score))
    return {"Status": "MATCH" if score >= threshold else "NO_MATCH", "Score": round(score, 6)}


def main():
    threshold = DEFAULT_THRESHOLD
    try:
        if len(sys.argv) != 3 or sys.argv[1] != "--model-dir":
            raise Rejected("UNAVAILABLE", "CONFIGURATION")
        raw = sys.stdin.buffer.read(MAX_REQUEST_BYTES + 1)
        if len(raw) > MAX_REQUEST_BYTES:
            raise Rejected("INVALID_IMAGE", "REQUEST_TOO_LARGE")
        try:
            request = json.loads(raw)
        except (ValueError, UnicodeError):
            raise Rejected("INVALID_IMAGE", "INVALID_JSON") from None
        if not isinstance(request, dict):
            raise Rejected("INVALID_IMAGE", "INVALID_JSON")
        value = request.get("Threshold", DEFAULT_THRESHOLD)
        if isinstance(value, bool) or not isinstance(value, (int, float)) or not math.isfinite(value) or not 0.3 <= value <= 0.9:
            raise Rejected("UNAVAILABLE", "THRESHOLD_CONFIGURATION")
        threshold = float(value)
        result = compare(request, pathlib.Path(sys.argv[2]), threshold)
    except Rejected as exc:
        result = {"Status": exc.status, "DetailCode": exc.detail}
    except Exception:
        # Native exception text can include paths or image content; expose a code only.
        result = {"Status": "UNAVAILABLE", "DetailCode": "WORKER_ERROR"}
    result.update(Threshold=threshold, ModelVersion=MODEL_VERSION)
    print(json.dumps(result, separators=(",", ":"), allow_nan=False), flush=True)


if __name__ == "__main__":
    main()
