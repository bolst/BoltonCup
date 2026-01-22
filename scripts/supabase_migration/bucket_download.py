import os
import boto3
import zipfile
from pathlib import Path
from dotenv import load_dotenv
from botocore.exceptions import ClientError



def download_s3_bucket_as_zip(
    bucket_name: str,
    output_zip_path: str,
    aws_access_key_id: str = None,
    aws_secret_access_key: str = None,
    region_name: str = None,
    endpoint_url: str = None,
    prefix: str = "",
):
    """
    Download all files from an S3 bucket and create a ZIP file.
    
    Args:
        bucket_name: Name of the S3 bucket
        output_zip_path: Path where the ZIP file will be saved
        aws_access_key_id: AWS access key (optional, will use env vars if not provided)
        aws_secret_access_key: AWS secret key (optional, will use env vars if not provided)
        region_name: AWS region (optional, will use env vars if not provided)
        endpoint_url: S3 endpoint URL (needed for non-AWS S3, e.g. Supabase, MinIO)
        prefix: Optional prefix to filter objects in the bucket
    """
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
    
    s3_client = boto3.client('s3', **session_kwargs)
    
    # Create output directory if it doesn't exist
    output_path = Path(output_zip_path)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    
    print(f"Downloading files from bucket '{bucket_name}'...")
    
    # Create ZIP file
    with zipfile.ZipFile(output_zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        # Paginate through all objects in the bucket
        paginator = s3_client.get_paginator('list_objects_v2')
        page_iterator = paginator.paginate(Bucket=bucket_name, Prefix=prefix)
        
        file_count = 0
        for page in page_iterator:
            if 'Contents' not in page:
                print(f"No files found in bucket '{bucket_name}' with prefix '{prefix}'")
                continue
            
            for obj in page['Contents']:
                key = obj['Key']
                
                # Skip directories (keys ending with '/')
                if key.endswith('/'):
                    continue
                try:
                    # Download file to memory
                    print(f"Downloading: {key}")
                    response = s3_client.get_object(Bucket=bucket_name, Key=key)
                    file_data = response['Body'].read()
                    
                    # Add to ZIP file
                    zipf.writestr(key, file_data)
                    file_count += 1
                    
                except ClientError as e:
                    print(f"Error downloading {key}: {e}")
                    continue
        
        print(f"\nSuccessfully downloaded {file_count} files to {output_zip_path}")


if __name__ == "__main__":
    load_dotenv()
    
    AWS_ACCESS_KEY = os.getenv("AWS_ACCESS_KEY_ID")
    AWS_SECRET_KEY = os.getenv("AWS_SECRET_ACCESS_KEY")
    AWS_REGION = os.getenv("AWS_REGION", "us-east-1")
    S3_ENDPOINT_URL = os.getenv("S3_ENDPOINT_URL")
    
    os.makedirs('buckets', exist_ok=True)
    
    buckets = [
        "penalty-songs",
        "goal-horns",
        "sponsors",
        "logos",
        "images",
        "profile-pictures"
    ]
    
    for bucket in buckets:
        print(f"Downloading bucket '{bucket}'")
        download_s3_bucket_as_zip(
            bucket_name=bucket,
            output_zip_path=f"/buckets/{bucket}.zip",
            aws_access_key_id=AWS_ACCESS_KEY,
            aws_secret_access_key=AWS_SECRET_KEY,
            region_name=AWS_REGION,
            endpoint_url=S3_ENDPOINT_URL,
        )