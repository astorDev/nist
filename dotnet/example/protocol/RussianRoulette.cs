namespace Nist.Example;

public partial class Uris {
    public const string RussianRouletteShot = "russian-roulette-shot";
}

public record Shot(bool Idle);

public record Gun(int Size, int BulletIndex);

public partial class Errors {
    public static readonly Error Killed = new(HttpStatusCode.BadRequest, "Killed");
}

public partial class Client {
    public async Task<Shot> GetRussianRouletteShot() => await Get<Shot>(Uris.RussianRouletteShot);
    public async Task<Shot> PostRussianRouletteShot(Gun gun) => await Post<Shot>(Uris.RussianRouletteShot, gun);
}