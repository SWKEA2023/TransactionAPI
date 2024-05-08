# Setup

## Setup User secrets for the project
1. Open the project in Visual Studio
1. Right click on the project and select `Manage User Secrets`
1. Add the following secrets to the `secrets.json` file:
	```json
		{
	  "RabbitMQ": {
		"RMQ_URL": "amqp://<username>:<password>@<hostname>:5672/"
	  }
	}
	```