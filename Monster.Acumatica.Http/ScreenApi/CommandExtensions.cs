using System.Collections.Generic;
using Monster.Acumatica.BankImportApi;

namespace Monster.Acumatica.ScreenApi
{
    public static class CommandExtensions
    {
        //CA306500Actions
        //var schema = PX.Soap.Helper.GetSchema<CA306500Actions>(context);


        public static List<Command> AddCommand(
                this List<Command> current, Field field, string value)
        {
            current.Add(new Value()
            {
                Value = value,
                LinkedCommand = field
            });
            return current;
        }

        public static List<Command> AddCommand(
                this List<Command> current, Action action)
        {
            current.Add(action);
            return current;
        }

        public static List<Command> AddCommand(
                this List<Command> current, Command command)
        {
            current.Add(command);
            return current;
        }


        public static List<Command> AddCommand(
                this List<Command> current, Field field)
        {
            current.Add(field);
            return current;
        }
    }
}
