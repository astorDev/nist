[Route(Uris.Weapons)]
public class WeaponController{
    static readonly Weapon[] Accessible = { new Weapon("hammer", 350) };

    [HttpGet("{name}")]
    public async Task<Weapon> Get(string name){
        return Accessible.FirstOrDefault(w => w.Name.ToLower() == name.ToLower()) ?? throw new InvalidOperationException("weapon not found");
    }
}