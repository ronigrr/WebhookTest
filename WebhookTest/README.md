To run the program make sure you have:
- docker installed
- dotnet 9 installed

after that run redis with this command 
- docker run -d --name redis-local -p 6379:6379 redis:latest

now debug the app or run with
dotnet WebhookTest.dll