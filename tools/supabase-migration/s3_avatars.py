# %%
import os
import pandas as pd
from sqlalchemy import create_engine
from dotenv import load_dotenv
load_dotenv()

DATABASE_URL = os.environ["ConnectionString"]
engine = create_engine(DATABASE_URL)

sql_query = """
SELECT
    first_name || ' ' || last_name AS name,
    profile_picture,
    replace(
        profile_picture,
        'https://iiedqecnfyojvubvugmy.supabase.co/storage/v1/object/public/profile-pictures/',
        ''
    ) AS profile_picture_key,
    id,
    avatar_key
FROM
    core.accounts
"""

df = pd.read_sql(sql_query, engine)
df.head()

# %%
import boto3
from botocore.exceptions import ClientError

s3_supabase = boto3.client(
    's3',
    endpoint_url=os.environ['SUPABASE_ENDPOINT'],
    aws_access_key_id=os.environ['SUPABASE_ACCESS_KEY'],
    aws_secret_access_key=os.environ['SUPABASE_SECRET_KEY'],
    region_name='us-west-1'
)

# Client for Cloudflare R2
s3_cloudflare = boto3.client(
    's3',
    endpoint_url=os.environ['CLOUDFLARE_ENDPOINT'],
    aws_access_key_id=os.environ['CLOUDFLARE_ACCESS_KEY'],
    aws_secret_access_key=os.environ['CLOUDFLARE_SECRET_KEY'],
    region_name='wnam'
)

# %%
for row in df.itertuples(index=False):
    source_key = row.profile_picture_key
    dest_key = row.avatar_key
    
    # Skip rows where either key might be null/missing in the database
    if pd.isna(source_key) or pd.isna(dest_key):
        print(f"Skipping row for user {row.id}: Missing source or destination key.")
        continue
        
    try:
        # Step A: Download from Supabase
        print(f"Transferring {source_key} -> {dest_key}...")
        
        response = s3_supabase.get_object(Bucket=os.environ['SUPABASE_BUCKET'], Key=source_key)
        file_stream = response['Body'].read()
        
        # Step B: Upload to Cloudflare R2
        s3_cloudflare.put_object(
            Bucket=os.environ['CLOUDFLARE_BUCKET'], 
            Key=dest_key, 
            Body=file_stream,
            ContentType=response.get('ContentType', 'image/webp') 
        )
        print(f"Successfully transferred user {row.id}'s avatar.")
        
    except ClientError as e:
        print(f"Error transferring {source_key} for user {row.id}: {e}")
    except Exception as e:
        print(f"An unexpected error occurred for user {row.id}: {e}")

print("Transfer process complete.")

# %%



