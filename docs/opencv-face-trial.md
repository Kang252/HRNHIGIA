# OpenCV face match trial

The trial compares one remote IN/OUT check-in photo with that employee's active HR-approved reference. YuNet detects the face and SFace produces a cosine similarity score. Photos are sent only to the HRM application server and processed locally by a short-lived Python worker; the worker does not save photos, embeddings, or request logs. There is no per-request OpenCV API fee, although the server consumes CPU, memory, storage, and build time.

## Scope and consent

- The checkbox on each IN/OUT submission is optional. It records the employee's consent version and UTC time for that specific punch. Offline punches keep this choice in the encrypted browser queue and send it with the punch.
- The employee can explicitly request a comparison later from their pending punch. A reviewer may retry that request only after the employee consented.
- VISIT punches are not compared. A missing, pending, or revoked HR reference cannot be compared.
- A match is only a similarity score, not a probability, a verified identity, or evidence that a live person was in front of the camera. No liveness detection is included. Every IN/OUT remains pending until a person reviews it; OpenCV never approves attendance.
- Terminal image outcomes (match, no match, no face, multiple faces, low quality, invalid image) are saved for that punch. To try again, the employee must make a new punch with a new camera capture. Temporary busy, timeout, and unavailable outcomes may be retried.

## Runtime

The root Docker image installs Python and pinned `opencv-python-headless`/NumPy packages. During image build, `OpenCv/download_models.py` fetches the YuNet and SFace ONNX files from the pinned OpenCV Zoo commit and checks their exact byte size and SHA-256. Model binaries are deliberately excluded from Git and Docker build context until the verified download step.

Configuration:

| Variable | Default | Purpose |
|---|---|---|
| `HRM_OPENCV_ENABLED` | `false` in application configuration; `true` in the trial Docker image | Show and allow the employee's opt-in control |
| `HRM_OPENCV_PYTHON` | `python` on Windows, `python3` elsewhere | Python executable for the local worker |
| `HRM_OPENCV_MODEL_PATH` | `OpenCv/models` under application content root | Directory with the verified ONNX models |
| `HRM_OPENCV_THRESHOLD` | `0.45` | Cosine threshold; accepted configuration range `0.30`–`0.90` |

Each match launches one isolated worker, limits model work to one concurrent request per process, caps images at 2 MB each and 12 megapixels, and stops after 12 seconds. Images and embeddings are not written to disk or logs. The database migration stores consent, outcome, score, model version, and comparison time beside the punch; it never changes attendance status.

For a local trial, install Python 3 and the pinned `OpenCv/requirements.txt`, then run `python NHIGIA.Modern/OpenCv/download_models.py --model-dir NHIGIA.Modern/OpenCv/models`; set `HRM_OPENCV_ENABLED=true` and `HRM_OPENCV_MODEL_PATH` to that directory. Do not copy a developer's local model cache into deployment.

## Model and production review

The app pins OpenCV Zoo commit `47534e27c9851bb1128ccc0102f1145e27f23f98`. The repository marks the YuNet model MIT and the SFace directory Apache-2.0; the corresponding license texts are in `NHIGIA.Modern/OpenCv/licenses`. The upstream repository also notes that model terms and training-data provenance must be reviewed for a production use. Confirm those rights with the company before using employee biometrics beyond a limited technical trial.

Before relying on scores, collect a consented, representative test set under the company's privacy process and measure false matches and false rejections across lighting, camera, and employee groups. The default threshold is only a starting point; tune it using that evaluation. Keep human review and an alternate attendance path available if the worker is unavailable or a face cannot be compared. This trial does not satisfy a liveness requirement.

Technical references:

- [OpenCV face detection and recognition documentation](https://docs.opencv.org/4.x/d0/dd4/tutorial_dnn_face.html)
- [OpenCV Zoo YuNet](https://github.com/opencv/opencv_zoo/tree/main/models/face_detection_yunet) and [SFace](https://github.com/opencv/opencv_zoo/tree/main/models/face_recognition_sface)
- [SFace model provenance discussion](https://github.com/opencv/opencv_zoo/issues/313)
