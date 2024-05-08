# Setup

## Setup User secrets for the project

### With Visual Studio
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

### With .NET CLI
1. Open a terminal in the project directory
1. Run the following command:
	```bash
	dotnet user-secrets set "RabbitMQ:RMQ_URL" "amqp://<username>:<password>@<hostname>:5672/"
	```
1. Verify that the secret was added by running:
	```bash
	dotnet user-secrets list
	```
1. The output should be similar to:
	```bash
	RabbitMQ:RMQ_URL = amqp://<username>:<password>@<hostname>:5672/
	```