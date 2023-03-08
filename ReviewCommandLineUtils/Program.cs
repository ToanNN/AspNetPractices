using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

var app = new CommandLineApplication
{
    Name = "cool-npm",
    Description = "A Cool version of the node package manager",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue
};

app.HelpOption(inherited: true);
app.Command("config", configCommand =>
{
    //OnExecure is call if the config command is entered on the command line
    configCommand.OnExecute(() =>
    {
        Console.WriteLine("Please specify a subcommand");
        configCommand.ShowHelp();
        return 1;
    });

    configCommand.Command("set", setCommand =>
    {
        setCommand.Description = "set config value";
        var key = setCommand.Argument("key", "The key of the configuration setting").IsRequired();
        var value = setCommand.Argument("value", "The value of the configuration setting").IsRequired();

        setCommand.OnExecute(() =>
        {
            Console.WriteLine($"Setting a config setting {key.Value} = {value.Value}");
        });
    });

});


app.OnExecute(() =>
{
    Console.WriteLine("Specify a subcommand. Use -h to find more options");
    app.ShowHelp();
    return 1;
});

return app.Execute(args);

//return HandleRemainingArguments(args);

int HandleRemainingArguments(string[] strings)
{
    var app = new CommandLineApplication()
    {
        UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue
    };

    var optionMessage = app.Option("-m|--message <MSG>", "Required. The message", CommandOptionType.SingleValue)
        .IsRequired();

//var optionReceiver =
//    app.Option("--to <EMAIL>", "Required. The receiver's email address", CommandOptionType.SingleValue)
//        .IsRequired()
//        .Accepts(v => v.EmailAddress());
//var optionSender = app.Option("--from <EMAIL>", "Required. The sender.", CommandOptionType.SingleValue)
//    .IsRequired()
//    .Accepts(v => v.EmailAddress());

//var attachments = app.Option("--attachment <FILE>", "Files to attach", CommandOptionType.MultipleValue)
//    .Accepts(v => v.ExistingFile());

    var importance = app.Option("-i|--importance <IMPORTANCE>", "Possible values: low, medium, or high",
            CommandOptionType.SingleValue)
        .Accepts().Values("low", "medium", "high");

//var optionMaxSize = app
//    .Option<int>("--max-size <MB>", "The maximum size of the message in MB.", CommandOptionType.SingleValue)
//    .Accepts(o => o.Range(1, 50));

//var optionColor = app.Option("--color <COLOR>", "The color. Should be 'red' or 'blue'.", CommandOptionType.SingleValue);
//optionColor.Validators.Add(new MustBeBlueOrRedValidator());

    app.OnExecute(() =>
    {
        Console.WriteLine("Message = " + optionMessage.Value());

        var options = app.GetOptions();
        foreach (var commandOption in options)
        {
            Console.WriteLine(
                $"Short Name: {commandOption.ShortName} Long Name: {commandOption.LongName}, Value Name: {commandOption.ValueName}");
        }

        var importanceOption = options.FirstOrDefault(x => x.ShortName == "i");

        Console.WriteLine("Importance = " + importanceOption?.Value());

        var remainingArgs = app.RemainingArguments.ToArray();

        foreach (var remainingArg in remainingArgs)
        {
            Console.WriteLine("Remaining argument: " + remainingArg);
        }
    });

    return app.Execute(strings);
}

public class MustBeBlueOrRedValidator : IOptionValidator
{
    public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
    {
        // This validator only runs if there is a value
        if (!option.HasValue()) return ValidationResult.Success;
        var val = option.Value();

        if (val != "red" && val != "blue")
        {
            return new ValidationResult($"The value for --{option.LongName} must be 'red' or 'blue'");
        }

        return ValidationResult.Success;
    }
}