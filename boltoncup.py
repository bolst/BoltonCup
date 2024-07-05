games = 8
ppt = 15  # players per team
teams = 4
include_jerseys = 0 # 0 for no 1 for yes
ice_rate = 217.73
prize_per_player_first = 60
time_keeper_rate = 15
ref_rate = 30

print('\n\n')

expenses = {
            "ice rentals": ice_rate * games,
            "referees": ref_rate * games,
            "timekeepers": time_keeper_rate * games,
            "jerseys": 35*ppt*teams * include_jerseys,
            "prize pool": prize_per_player_first * ppt 
        }

for expense in expenses:
    print(expense, '\t\t', expenses[expense])

total = sum(expenses.values())
print('======================================')
print(f'Total Cost: \t\t {total}')
print(f'Cost/player ({ppt*teams}): \t {total/ppt//teams}')

print('\n\n')

