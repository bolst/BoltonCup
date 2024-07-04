games = 8
ppt = 15  # players per team
teams = 4
include_jerseys = 0 # 0 for no 1 for yes

print('\n\n')

expenses = {
            "ice rentals": 250 * games,
            "referees": 30 * games,
            "timekeepers": 15 * games,
            "jerseys": 35*ppt*teams * include_jerseys,
            "prize pool": 50 * ppt 
        }

for expense in expenses:
    print(expense, '\t\t', expenses[expense])

total = sum(expenses.values())
print('======================================')
print(f'Total Cost: \t\t {total}')
print(f'Cost/player ({ppt*teams}): \t {total/ppt//teams}')

print('\n\n')

