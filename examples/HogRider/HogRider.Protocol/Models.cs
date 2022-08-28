namespace HogRider.Protocol;

public record About(string Description, string Version, string Environment);

public record Weapon(string Name, int Power);