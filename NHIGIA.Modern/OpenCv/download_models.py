"""Provision pinned OpenCV Zoo models. Run at setup/build, never on a photo request."""
import argparse
import hashlib
import json
import pathlib
import tempfile
import urllib.request


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model-dir", required=True, type=pathlib.Path)
    args = parser.parse_args()
    manifest = json.loads(pathlib.Path(__file__).with_name("models.json").read_text())
    args.model_dir.mkdir(parents=True, exist_ok=True)
    for model in manifest["models"]:
        destination = args.model_dir / model["name"]
        if (destination.is_file() and destination.stat().st_size == model["size"]
                and hashlib.sha256(destination.read_bytes()).hexdigest() == model["sha256"]):
            print(f"Verified {model['name']}")
            continue
        url = (f"https://media.githubusercontent.com/media/opencv/opencv_zoo/"
               f"{manifest['commit']}/{model['path']}")
        temporary = None
        try:
            with tempfile.NamedTemporaryFile(dir=args.model_dir, delete=False) as output:
                temporary = pathlib.Path(output.name)
                digest, size = hashlib.sha256(), 0
                with urllib.request.urlopen(url, timeout=90) as response:
                    while block := response.read(1024 * 1024):
                        size += len(block)
                        if size > model["size"]:
                            raise ValueError("Model download exceeds pinned size")
                        digest.update(block)
                        output.write(block)
            if size != model["size"] or digest.hexdigest() != model["sha256"]:
                raise ValueError("Model checksum mismatch")
            temporary.replace(destination)
            print(f"Downloaded and verified {model['name']}")
        finally:
            if temporary is not None:
                temporary.unlink(missing_ok=True)


if __name__ == "__main__":
    main()
