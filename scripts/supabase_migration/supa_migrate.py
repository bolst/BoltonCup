# %%
from sqlalchemy import create_engine, text
from sqlalchemy.orm import Session
import pandas as pd
import pandas.io.sql as sqlio
import os
from dotenv import load_dotenv

# %%
load_dotenv()

supa_cstring = os.environ["SUPABASE_CONNECTION_STRING"]
prod_cstring = os.environ["PROD_CONNECTION_STRING"]

supa_engine = create_engine(supa_cstring)
prod_engine = create_engine(prod_cstring)

# %%
# get all data
# memory limits only exist if you're afraid
ordered_tables = ['tournament', 'account', 'team', 'players', 'game', 'points', 'penalties']
data: dict[str, pd.DataFrame] = {}
with supa_engine.connect() as supa_connection:
    for table in ordered_tables:
        data[table] = sqlio.read_sql_query(f"select * from {table}", supa_connection)

# %%
# define how schema should map into new one
mappings = {
    'tournament': {
        'table': 'tournaments',
        'new_columns': {},
        'columns': ['tournament_id', 'start_date', 'end_date', 'winning_team_id', 'name', 'current', 'registration_open', 'payment_open', 'player_payment_link', 'goalie_payment_link', 'player_limit', 'goalie_limit'],
        'renames': {
            'tournament_id': 'id',
            'current': 'is_active',
            'registration_open': 'is_registration_open',
            'payment_open': 'is_payment_open',
            'player_payment_link': 'skater_payment_link',
            'player_limit': 'skater_limit',
        }
    },
    'team': {
        'table': 'teams',
        'new_columns': {
            'abbreviation': 'BC'
        },
        'columns': ['id', 'name', 'primary_color_hex', 'secondary_color_hex', 'tertiary_color_hex', 'logo_url', 'name_short', 'tournament_id', 'gm_account_id', 'banner_image', 'goal_horn_url', 'penalty_song_url'],
        'renames': {
            'primary_color_hex': 'primary_hex',
            'secondary_color_hex': 'secondary_hex',
            'tertiary_color_hex': 'tertiary_hex',
            'banner_image': 'banner_url',
            'goal_horn_url': 'goal_song_url'
        },
    },
    'account': {
        'table': 'accounts',
        'new_columns': {},
        'columns': ['id', 'firstname', 'lastname', 'email', 'birthday', 'highestlevel', 'profilepicture', 'preferred_beer'],
        'renames': {
            'firstname': 'first_name',
            'lastname': 'last_name',
            'highestlevel': 'highest_level',
            'profilepicture': 'profile_picture',
        }
    },
    'players': {
        'table': 'players',
        'new_columns': {},
        'columns': ['id', 'account_id', 'team_id', 'jersey_number', 'position', 'tournament_id'],
        'renames': {}
    },
    'game': {
        'table': 'games',
        'new_columns': {},
        'columns': ['id', 'home_team_id', 'away_team_id', 'date', 'type', 'location', 'rink', 'tournament_id'],
        'renames': {
            'date': 'game_time',
            'type': 'game_type',
            'location': 'venue'
        }
    },
    'points': {
        'table': 'goals',
        'new_columns': {
            'period_label': 'TODO'
        },
        'columns': ['id', 'game_id', 'time', 'period', 'notes', 'scorer_id', 'assist1_player_id', 'assist2_player_id'],
        'renames': {
            'time': 'period_time_remaining',
            'period': 'period_number',
            'scorer_id': 'goal_player_id'
        }
    },
    'penalties': {
        'table': 'penalties',
        'new_columns': {
            'period_label': 'TODO'
        },
        'columns': ['id', 'time', 'period', 'duration_mins', 'infraction_name', 'notes', 'game_id', 'player_id'],
        'renames': {
            'time': 'period_time_remaining',
            'period': 'period_number',
        }
    }
}

# %%
with Session(autocommit=False, autoflush=False, bind=prod_engine) as prod_session:
    try:
        for table in ordered_tables:
            mapping = mappings[table]
            new_table_name = mapping['table']
            columns = mapping['columns']
            renames = mapping['renames']
            
            print(f"{'='*20} Migrating table '{table}' into '{new_table_name}' {'='*20}")

            # get columns and rename
            insert_data: pd.DataFrame = data[table][columns].rename(columns=renames)
            # add new columns
            for new_col_name, new_col_val in mapping['new_columns'].items():
                insert_data[new_col_name] = new_col_val
            total_rows = insert_data.shape[0]
            print(f"Inserting {total_rows} rows with columns:\n- {'\n- '.join(insert_data.columns)}")
            insert_data.to_sql(con=prod_session.connection(), name=new_table_name, schema='core', if_exists='append', index=False)
        
        # apply manual updates
        prod_session.execute(text(
            """UPDATE core.penalties
                SET
                    period_label = CASE period_number
                        WHEN 1 THEN '1st'
                        WHEN 2 THEN '2nd'
                        WHEN 3 THEN '3rd'
                        WHEN 4 THEN 'OT'
                        WHEN 5 THEN 'SO'
                    END"""
        ))
        prod_session.execute(text(
            """UPDATE core.goals
                SET
                    period_label = CASE period_number
                        WHEN 1 THEN '1st'
                        WHEN 2 THEN '2nd'
                        WHEN 3 THEN '3rd'
                        WHEN 4 THEN 'OT'
                        WHEN 5 THEN 'SO'
                    END"""
        ))
        
        print("Committing...")
        prod_session.commit()
    except Exception as e:
        print(f"Rolling back because of error...")
        prod_session.rollback()
        raise e
    print("Done!")

# %%



