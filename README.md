The Challenge
To create a basic telegram webhook bot in c# without using any bot frameworks (e.g. MicrosoftBotFramwork). You can however use a library wrapping the telegram API (e.g. https://github.com/TelegramBots/Telegram.Bot)

The bot should:

list all it's available commands when issued the /help or /about commands
have a command which returns random quotes from your favorite TV character
have a command which takes parameters
have a command which sends back options in button form
have a command which then awaits typed options from subsequent messages (e.g. /quotes which then accepts cryptocurrency pairs and returns the quote for latest price from an api)
at least one command which calls an external API
As you can see there is an opportunity to satisfy the last 3 requirements in a single command if you wish.
