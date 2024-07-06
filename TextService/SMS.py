import smtplib
carriers = {
	'att':    '@mms.att.net',
	'tmobile':' @tmomail.net',
	'verizon':  '@vtext.com',
	'sprint':   '@page.nextel.com',
    'bell':     '@txt.bell.ca',
    'rogers':   '@mms.rogers.com'
}

def send(message, to_number='5198176511'):
        # Replace the number with your own, or consider using an argument\dict for multiple people.
	to_number = f"{to_number}{carriers['bell']}"
	auth = ('nicbolton903@gmail.com', 'xbyf fclm zods uoyy')

	# Establish a secure session with gmail's outgoing SMTP server using your gmail account
	server = smtplib.SMTP( "smtp.gmail.com", 587 )
	server.starttls()
	server.login(auth[0], auth[1])

	# Send text message through SMS gateway of destination number
	server.sendmail( auth[0], to_number, message)
 
if __name__ == '__main__':
    text = 'test123'
    send(text,to_number='5195663730')