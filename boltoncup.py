games = 8
ppt = 16                 # players per team
teams = 4
include_jerseys = 0      # 0 for no, 1 for yes
ice = 1868.36
ppp1 = 60                # prize per player on 1st place team
time_keeper_rate = 15
ref_rate = 35

print('\n\n')

expenses = {
            "ice rentals": ice,
            "referees": ref_rate * games,
            "timekeepers": time_keeper_rate * games,
            "jerseys": 35*ppt*teams * include_jerseys,
            "prize pool": ppp1 * ppt 
        }

for expense in expenses:
    print(expense, '\t\t', expenses[expense])

total = sum(expenses.values())
print('======================================')
print(f'Total Cost: \t\t {total}')
print(f'Cost/player ({ppt*teams}): \t {total/ppt//teams}')

print('\n\n')

