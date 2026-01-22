import os
import boto3
import zipfile
import mimetypes
from pathlib import Path
from dotenv import load_dotenv
from botocore.exceptions import ClientError

def upload_zip_contents_to_s3(
    zip_path: str,
    bucket_name: str,
    aws_access_key_id: str = None,
    aws_secret_access_key: str = None,
    region_name: str = None,
    endpoint_url: str = None,
    prefix: str = "",
):
    """
    Upload contents of a ZIP file to an S3 bucket, maintaining file structure.
    """
    if not os.path.exists(zip_path):
        print(f"File not found: {zip_path}")
        return

    # Initialize S3 client
    session_kwargs = {}
    if aws_access_key_id:
        session_kwargs['aws_access_key_id'] = aws_access_key_id
    if aws_secret_access_key:
        session_kwargs['aws_secret_access_key'] = aws_secret_access_key
    if region_name:
        session_kwargs['region_name'] = region_name
    if endpoint_url:
        session_kwargs['endpoint_url'] = endpoint_url
    
    try:
        s3_client = boto3.client('s3', **session_kwargs)
    except Exception as e:
        print(f"Failed to create S3 client: {e}")
        return

    # Check if bucket exists, create if not
    try:
        s3_client.head_bucket(Bucket=bucket_name)
    except ClientError:
        print(f"Bucket '{bucket_name}' not found. Attempting to create...")
        try:
            if region_name and region_name != 'us-east-1':
                s3_client.create_bucket(
                    Bucket=bucket_name,
                    CreateBucketConfiguration={'LocationConstraint': region_name}
                )
            else:
                 s3_client.create_bucket(Bucket=bucket_name)
            print(f"Created bucket '{bucket_name}'")
        except ClientError as e:
            print(f"Could not create bucket '{bucket_name}': {e}")
            return

    print(f"Uploading files from {zip_path} to bucket '{bucket_name}' with prefix '{prefix}'...")
    
    try:
        with zipfile.ZipFile(zip_path, 'r') as zipf:
            files_to_upload = [f for f in zipf.namelist() if not f.endswith('/')]
            total_files = len(files_to_upload)
            
            for i, filename in enumerate(files_to_upload, 1):
                # Read file from zip
                file_data = zipf.read(filename)
                
                # Guess content type
                content_type, _ = mimetypes.guess_type(filename)
                if not content_type:
                    content_type = 'application/octet-stream'
                
                # Construct S3 key with prefix
                key = f"{prefix}/{filename}" if prefix else filename
                # Remove double slashes if any (e.g. if filename starts with /)
                key = key.replace('//', '/')
                
                print(f"[{i}/{total_files}] Uploading: {key}")
                
                s3_client.put_object(
                    Bucket=bucket_name,
                    Key=key,
                    Body=file_data,
                    ContentType=content_type
                )
                
        print(f"Successfully uploaded contents of {zip_path} to {bucket_name}")
        
    except zipfile.BadZipFile:
        print(f"Error: {zip_path} is not a valid zip file")
    except Exception as e:
        print(f"Error uploading files: {e}")

if __name__ == "__main__":
    load_dotenv()
    
    # Destination S3 Configuration
    DEST_ACCESS_KEY = os.environ["DEST_AWS_ACCESS_KEY_ID"]
    DEST_SECRET_KEY = os.environ["DEST_AWS_SECRET_ACCESS_KEY"]
    DEST_REGION = os.environ["DEST_AWS_REGION"]
    DEST_ENDPOINT = os.environ["DEST_S3_ENDPOINT_URL"]
    
    # For zip files found at the root 'buckets/' level with no folder, uploads go to this bucket
    DEFAULT_ROOT_BUCKET = "assets"

    ZIP_DIR = "buckets"
    
    if not os.path.exists(ZIP_DIR):
        print(f"Directory '{ZIP_DIR}' not found. Please run bucket_download.py first or ensure the directory exists.")
        exit(1)
        
    print(f"Scanning directory '{ZIP_DIR}' for zip files...")
    
    for root, dirs, files in os.walk(ZIP_DIR):
        for file in files:
            if not file.endswith('.zip'):
                continue
                
            zip_path = os.path.join(root, file)
            
            # Determine logic based on path
            # rel_path is path relative to ZIP_DIR (e.g. "players/profile-pictures.zip" or "images.zip")
            rel_path = os.path.relpath(zip_path, ZIP_DIR)
            path_parts = Path(rel_path).parts
            
            # e.g. "profile-pictures" from "profile-pictures.zip"
            zip_name_stem = Path(file).stem 
            
            if len(path_parts) > 1:
                # File is in a subdirectory (e.g. buckets/players/profile-pictures.zip)
                # Bucket = subdirectory name (e.g. "players")
                # Prefix = zip name (e.g. "profile-pictures")
                bucket_name = path_parts[0]
                prefix = zip_name_stem
                print(f"Found nested zip: {rel_path} -> Bucket: {bucket_name}, Prefix: {prefix}/")
                
            else:
                # File is at root (e.g. buckets/images.zip)
                # content prefix = zip name (e.g. "images")
                bucket_name = DEFAULT_ROOT_BUCKET
                prefix = zip_name_stem
                print(f"Found root zip: {rel_path} -> Bucket: {bucket_name} (default), Prefix: {prefix}/")
        
            upload_zip_contents_to_s3(
                zip_path=zip_path,
                bucket_name=bucket_name,
                aws_access_key_id=DEST_ACCESS_KEY,
                aws_secret_access_key=DEST_SECRET_KEY,
                region_name=DEST_REGION,
                endpoint_url=DEST_ENDPOINT,
                prefix=prefix
            )
