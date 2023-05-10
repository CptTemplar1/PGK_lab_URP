using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum VolumetricFogMode
{
    Off,
    On
}

public enum FogMaskBlendMode
{
    Additive,
    Exclusive
}

[Serializable]
public sealed class VolumetricFogModeParameter : VolumeParameter<VolumetricFogMode>
{
    public VolumetricFogModeParameter(VolumetricFogMode value, bool overrideState = false) : base(value, overrideState) { }
}

[Serializable]
public sealed class FogMaskBlendModeParameter : VolumeParameter<FogMaskBlendMode>
{
    public FogMaskBlendModeParameter(FogMaskBlendMode value, bool overrideState = false) : base(value, overrideState) { }
}

[Serializable, VolumeComponentMenuForRenderPipeline("Volumetrics/Volumetric Fog", typeof(UniversalRenderPipeline))]
public sealed class VolumetricFog : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Ustaw na ON., aby w³¹czyæ mg³ê wolumetryczn¹.")]
    public VolumetricFogModeParameter mode = new VolumetricFogModeParameter(VolumetricFogMode.Off);

    // Renderowanie wydajnoœciowe i bazowe

    [Tooltip("Okreœla liczbê punktów u¿ywanych do ca³kowania wolumetrycznej objêtoœci mg³y. Wy¿sze wartoœci s¹ bardziej kosztowne obliczeniowo, ale ni¿sze wartoœci mog¹ powodowaæ artefakty.")]
    public ClampedIntParameter sampleCount = new ClampedIntParameter(64, 8, 128);

    [Tooltip("Po w³¹czeniu pozycje próbek bêd¹ losowo dostosowywane w ka¿dej klatce. Zastêpuje to szum statyczny szumem dynamicznym.")]
    public BoolParameter animateSamplePosition = new BoolParameter(false);

    [Tooltip("Po w³¹czeniu mg³a realistycznie t³umi œwiat³o z g³ównego œwiat³a kierunkowego. Self Shadowing jest kosztowny obliczeniowo, ale wygl¹da bardziej realistycznie.")]
    public BoolParameter selfShadowingEnabled = new BoolParameter(true);

    [Tooltip("Self Shadowing Octaves s¹ niezwykle wydajne. Zwykle wystarcza tylko 1 oktawa samocieniaj¹ca. Jednak wiêcej samoocieniaj¹cych siê oktaw daje bardziej realistyczne rezultaty. W³asne cieniowanie nigdy nie u¿yje wiêcej oktaw ni¿ podstawowe renderowanie szumów (ustawione w Volumetric Noise poni¿ej).")]
    public ClampedIntParameter maximumSelfShadowingOctaves = new ClampedIntParameter(1, 1, 3);

    [Tooltip("Po w³¹czeniu mg³a bêdzie zacieniona, gdy g³ówne œwiat³o znajdzie siê poni¿ej linii horyzontu. Horizon Shadowing jest kosztowny obliczeniowo, ale wygl¹da bardziej realistycznie.")]
    public BoolParameter horizonShadowingEnabled = new BoolParameter(true);

    [Tooltip("Odleg³oœæ, przy której renderer mg³y prze³¹czy siê z mg³y wolumetrycznej na analityczn¹. Nie wp³ywa to na wydajnoœæ, ale mo¿e wp³yn¹æ na jakoœæ renderowania mg³y. Mniejsze wartoœci powoduj¹, ¿e renderer pokonuje mniejsz¹ odleg³oœæ miêdzy próbkami mg³y, co skutkuje wy¿sz¹ jakoœci¹ i mniejsz¹ liczb¹ artefaktów.")]
    public MinFloatParameter maxDistanceVolumetric = new MinFloatParameter(64, 10);

    [Tooltip("Mg³a analityczna zastêpuje mg³ê wolumetryczn¹ po odleg³oœci próbkowania mg³y wolumetrycznej. Wy³¹czenie mg³y analitycznej mo¿e poprawiæ wydajnoœæ.")]
    public BoolParameter analyticFogEnabled = new BoolParameter(false);

    [Tooltip("Maksymalna odleg³oœæ, która bêdzie symulowana dla mg³y analitycznej. Nie wp³ywa to na wydajnoœæ, ale mo¿e wp³yn¹æ na wizualn¹ charakterystykê sceny.")]
    public MinFloatParameter maxDistanceAnalytic = new MinFloatParameter(5000, 100);

    [Tooltip("W³¹cza Temporal Anti-Aliasing, który mo¿e poprawiæ jakoœæ mg³y w scenariuszach z gêst¹ mg³¹.")]
    public BoolParameter temporalAntiAliasingEnabled = new BoolParameter(false);

    [Tooltip("Okreœla wa¿noœæ najnowszych klatek integrowania mg³y. Ni¿sze wartoœci oznaczaj¹, ¿e wiêcej danych jest u¿ywanych z pamiêci podrêcznej danych historycznych.")]
    public ClampedFloatParameter temporalAntiAliasingIntegrationRate = new ClampedFloatParameter(0.03f, 0.01f, 0.99f);

    [Tooltip("Pozwala kontrolowaæ, czy maski przeciwmgielne maj¹ byæ dodawane, czy tylko renderowane")]
    public FogMaskBlendModeParameter fogMaskBlendMode = new FogMaskBlendModeParameter(FogMaskBlendMode.Additive);


    // Parametry mg³y
    [Tooltip("Gêstoœæ mg³y na scenie.")]
    public MinFloatParameter fogDensity = new MinFloatParameter(10, 0);

    [Tooltip("Okreœla kierunek, w którym œwiat³o bêdzie siê rozpraszaæ. Wartoœci ujemne powoduj¹ rozproszenie œwiat³a do ty³u. Wartoœci dodatnie powoduj¹ rozproszenie œwiat³a do przodu.")]
    public ClampedFloatParameter anisotropy = new ClampedFloatParameter(0.2f, -1, 1);

    [Tooltip("Mno¿nik, który wp³ywa na intensywnoœæ efektów œwietlnych mg³y w niezacienionych obszarach. Zwiêkszenie tego powoduje, ¿e oœwietlenie mg³y staje siê jaœniejsze w oœwietlonych obszarach. Powinien byæ u¿ywany tylko do efektów stylizowanych. [Domyœlnie: 1]")]
    public MinFloatParameter lightIntensity = new MinFloatParameter(1, 0);

    [Tooltip("Mno¿nik wp³ywaj¹cy na gêstoœæ mg³y w zacienionych obszarach. Zwiêkszenie tego powoduje, ¿e mg³a staje siê gêstsza w zacienionych obszarach. W efekcie powoduje to, ¿e zacienione obszary staj¹ siê bardziej widoczne. Nale¿y jednak uwa¿aæ, poniewa¿ mo¿e to równie¿ spowodowaæ, ¿e zacienione obszary stan¹ siê bardziej nieprzejrzyste. Powinien byæ u¿ywany tylko do efektów stylizowanych. [Domyœlnie: 1]")]
    public MinFloatParameter shadowIntensity = new MinFloatParameter(1, 0);


    // Geometria
    [Tooltip("Definiuje \"pod³ogê\" objêtoœci mg³y. Opadanie mg³y zaczyna siê dopiero po przekroczeniu tej wysokoœci przez mg³ê.")]
    public FloatParameter baseHeight = new FloatParameter(0);

    [Tooltip("Okreœla wysokoœæ, na której gêstoœæ mg³y zostanie os³abiona do 36% ustawionej gêstoœci mg³y. Jest to zawsze obliczane na podstawie wysokoœci bazowej. Na przyk³ad, jeœli twoja wysokoœæ podstawowa wynosi 10, a rozmiar granicy t³umienia wynosi 15, twoja mg³a bêdzie mia³a pe³n¹ gêstoœæ do wysokoœci y = 10 jednostek, póŸniej zmniejszy siê do 36% pierwotnej gêstoœci.")]
    public MinFloatParameter attenuationBoundarySize = new MinFloatParameter(10, 1);


    // Stylizacja
    [Tooltip("Tekstura, której mo¿na u¿yæ do nadania mgle wartoœci emisji i manipulowania kolorem mg³y w oœwietlonych i zacienionych obszarach. Zwykle u¿ywany do efektów stylizowanych.")]
    public Texture2DParameter colorRamp = new Texture2DParameter(null);

    [Tooltip("Kontroluje stopieñ wp³ywu palety kolorów na podstawowe wyniki fizyczne. Wartoœæ 0 oznacza, ¿e zmiana koloru jest ca³kowicie ignorowana. Wartoœæ 1 oznacza, ¿e odnosimy siê wy³¹cznie do palety kolorów")]
    public ClampedFloatParameter colorRampInfluence = new ClampedFloatParameter(0, 0, 1);


    // Szum
    [Tooltip("Tekstura 3D, która zostanie u¿yta do zdefiniowania intensywnoœci mg³y. Powtarza siê w domenie kafelkowania szumu (tiling). Wartoœæ 0 oznacza, ¿e gêstoœæ mg³y jest os³abiona do 0. Wartoœæ 1 oznacza, ¿e gêstoœæ mg³y nie jest os³abiana i odpowiada wartoœci ustawionej w parametrze Gêstoœæ mg³y.")]
    public Texture3DParameter noiseTexure = new Texture3DParameter(null);

    [Tooltip("Stopniowo ponownie próbkuje teksturê szumu w mniejszych domenach kafelkowych, aby zwiêkszyæ poziom szczegó³owoœci ostatecznej prezentacji mg³y. Wiêcej oktaw jest bardziej kosztowne obliczeniowo, ale zwiêksza poziom realizmu.")]
    public ClampedIntParameter octaves = new ClampedIntParameter(2, 1, 3);

    [Tooltip("Wartoœæ, o któr¹ czêstotliwoœæ próbkowania tekstury bêdzie wzrastaæ z ka¿d¹ kolejn¹ oktaw¹.")]
    public ClampedFloatParameter lacunarity = new ClampedFloatParameter(2, 1, 8);

    [Tooltip("Wartoœæ, o któr¹ amplituda tekstury bêdzie siê zmniejszaæ z ka¿d¹ kolejn¹ oktaw¹.")]
    public ClampedFloatParameter gain = new ClampedFloatParameter(0.3f, 0, 1);

    [Tooltip("D³ugoœæ ka¿dego boku szeœcianu, która opisuje szybkoœæ powtarzania siê tekstury szumu. Innymi s³owy, skala tekstury szumu w metrach.")]
    public MinFloatParameter noiseTiling = new MinFloatParameter(50, 0);

    [Tooltip("Szybkoœæ, z jak¹ szum bêdzie przesuwa³ siê w czasie. U¿yj tego do symulacji wiatru. Mierzona w metrach na sekundê.")]
    public Vector3Parameter noiseWindSpeed = new Vector3Parameter(new Vector3(0, 0, 0));

    [Tooltip("Odwzorowuje teksturê wejœciow¹ szumu z oryginalnego zakresu [0, 1] na nowy zakres zdefiniowany przez [NoiseIntensityMin, NoiseIntensityMax]. Na przyk³ad mapowanie [0,2, 0,8] spowoduje ponowne mapowanie szumu przez obciêcie dowolnych wartoœci poni¿ej 0,2 i powy¿ej 0,8, a nastêpnie ponowne odwzorowanie pozosta³ego zakresu od 0,2 do 0,8 z powrotem do 0,0 do 1,0 w celu zachowania szczegó³ów.")]
    public FloatRangeParameter noiseMap = new FloatRangeParameter(new Vector2(0, 1), 0, 1);


    public bool IsActive()
    {
        if (mode.value != VolumetricFogMode.On || fogDensity.value <= 0)
            return false;

        return true;
    }

    public bool IsTileCompatible() => false;
}
