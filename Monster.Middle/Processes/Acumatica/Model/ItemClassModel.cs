using Monster.Acumatica.Api.Reference;

namespace Monster.Middle.Processes.Acumatica.Model
{
    public class ItemClassModel
    {
        public string ItemClass { get; set; }
        public string PostingClass { get; set; }

        public ItemClassModel(ItemClass input)
        {
            ItemClass = input.ClassID.value;
            PostingClass = input.PostingClass.value;
        }
    }
}
