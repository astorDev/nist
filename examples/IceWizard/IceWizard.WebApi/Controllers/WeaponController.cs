[Route(Uris.Weapons)]
public class WeaponController {
    static readonly Weapon[] Accessible = { new Weapon("Ice", 100) };

    [HttpGet("{name}")]
    public Weapon Get(string name){
        return Accessible.FirstOrDefault(w => w.Name.ToLower() == name.ToLower()) ?? throw new WeaponNotExistsException();
    }
}