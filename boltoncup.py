games = 8
ppt = 15                 # players per team
teams = 4
ice = 1868.36
time_keeper_rate = 15
ref_rate = 35
win_ratio = 3/4 # ratio of 1st place winnings
player_rate = 50
goalie_rate = 25

print('\n\n')

expenses = {
            "ice rentals": ice,
            "referees": ref_rate * games,
            "timekeepers": time_keeper_rate * games
        }
revenue = {
            "players": ppt*4*player_rate,
            "goalies": 1*4*goalie_rate
        }

for expense in expenses:
    print(expense, '\t\t', expenses[expense])

e = sum(expenses.values())
r = sum(revenue.values())

print('======================================')
print(f'Total Cost:  \t\t {"%.2f" % e}')
print(f'Player Fees: \t\t {"%.2f" % r}')
print('')
print(f'Remaining:   \t\t {"%.2f" % (r-e)}')
print(f'1st place:   \t\t {"%.2f" % ((r-e)/ppt * win_ratio)}')
print(f'2nd place:   \t\t {"%.2f" % ((r-e)/ppt * (1-win_ratio))}')

print('\n\n')

