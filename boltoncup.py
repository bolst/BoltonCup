import os

os.system('clear')

games = 8
ppt = 15                 # players per team
teams = 4
ice = 1868.36 + 233
time_keeper_rate = 15
ref_rate = 0
win_ratio = 1 # ratio of 1st place winnings
player_rate = 50
goalie_rate = 25

expenses = {
            "ice rentals": ice,
            "referees": ref_rate * games,
            "timekeepers": time_keeper_rate * games
        }
revenue = {
            "players": ppt*4*player_rate,
            "goalies": 1*4*goalie_rate
        }

gifts = {
            "beer store gift card": 50
        }

for expense in expenses:
    print(expense, '\t\t', expenses[expense])

e = sum(expenses.values())
r = sum(revenue.values())
g = sum(gifts.values())

remaining = r-e+g

print('======================================')
print(f'Total Cost:  \t\t {"%.2f" % e}')
print(f'Player Fees: \t\t {"%.2f" % r}')
print(f'Gifts:       \t\t {"%.2f" % g}')
print('')
print(f'Remaining:   \t\t {"%.2f" % (remaining)}')
print(f'Winners({ppt+1}):   \t\t {"%.2f" % ((remaining)/ppt * win_ratio)}')

print('\n\n')

