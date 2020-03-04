using System.Text;

namespace RunningDinner
{
    public class Team
    {
        public string Name { get; set; }
        public string RoomNumber { get; set; }
        public string Email { get; set; }
        public string Phonenumber { get; set; }
        public string Teammember1 { get; set; }
        public string Teammember2 { get; set; }
        public string AdditionalFoodInfo { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"Teamname: {this.Name}\n");
            builder.Append($"RoomNumber: {this.RoomNumber}\n");
            builder.Append($"Email: {this.Email}\n");
            builder.Append($"Phonenumber: {this.Phonenumber}\n");
            builder.Append($"Teammember1: {this.Teammember1}\n");
            builder.Append($"Teammember2: {this.Teammember2}\n");
            builder.Append($"AdditionalFoodInfo: {this.AdditionalFoodInfo}\n");
            return builder.ToString();
        }
    }
}