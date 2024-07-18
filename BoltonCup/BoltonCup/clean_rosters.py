import json

def team_file(n:int) -> str: return f'wwwroot/team{n}.json'

def read_file(n:int) -> dict:
    with open(team_file(n), 'r') as rf:
        return json.load(rf)

def write_file(n:int, data):
    with open(team_file(n), 'w') as wf:
        json.dump(data, wf)

def clean_files():
    for n in [1,2,3,4]:
        data = read_file(n)
        data['players'] = []
        write_file(n, data)


if __name__ == '__main__':
    clean_files()
