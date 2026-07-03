namespace EcoMeal.Api.Entities
{
    public class User 
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Contact { get; set; }
    }
}
