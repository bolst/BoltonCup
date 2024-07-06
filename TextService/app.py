from flask import Flask, request
import SMS

app = Flask(__name__)


@app.route('/send', methods=['POST'])
def send_message():
    numbers = request.json['numbers']
    message = request.json['message']
    for number in numbers:
        SMS.send(message, to_number=number)
    
    return 'success'