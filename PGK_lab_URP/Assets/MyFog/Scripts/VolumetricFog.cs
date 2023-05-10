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
    [Tooltip("Ustaw na ON., aby w��czy� mg�� wolumetryczn�.")]
    public VolumetricFogModeParameter mode = new VolumetricFogModeParameter(VolumetricFogMode.Off);

    // Renderowanie wydajno�ciowe i bazowe

    [Tooltip("Okre�la liczb� punkt�w u�ywanych do ca�kowania wolumetrycznej obj�to�ci mg�y. Wy�sze warto�ci s� bardziej kosztowne obliczeniowo, ale ni�sze warto�ci mog� powodowa� artefakty.")]
    public ClampedIntParameter sampleCount = new ClampedIntParameter(64, 8, 128);

    [Tooltip("Po w��czeniu pozycje pr�bek b�d� losowo dostosowywane w ka�dej klatce. Zast�puje to szum statyczny szumem dynamicznym.")]
    public BoolParameter animateSamplePosition = new BoolParameter(false);

    [Tooltip("Po w��czeniu mg�a realistycznie t�umi �wiat�o z g��wnego �wiat�a kierunkowego. Self Shadowing jest kosztowny obliczeniowo, ale wygl�da bardziej realistycznie.")]
    public BoolParameter selfShadowingEnabled = new BoolParameter(true);

    [Tooltip("Self Shadowing Octaves s� niezwykle wydajne. Zwykle wystarcza tylko 1 oktawa samocieniaj�ca. Jednak wi�cej samoocieniaj�cych si� oktaw daje bardziej realistyczne rezultaty. W�asne cieniowanie nigdy nie u�yje wi�cej oktaw ni� podstawowe renderowanie szum�w (ustawione w Volumetric Noise poni�ej).")]
    public ClampedIntParameter maximumSelfShadowingOctaves = new ClampedIntParameter(1, 1, 3);

    [Tooltip("Po w��czeniu mg�a b�dzie zacieniona, gdy g��wne �wiat�o znajdzie si� poni�ej linii horyzontu. Horizon Shadowing jest kosztowny obliczeniowo, ale wygl�da bardziej realistycznie.")]
    public BoolParameter horizonShadowingEnabled = new BoolParameter(true);

    [Tooltip("Odleg�o��, przy kt�rej renderer mg�y prze��czy si� z mg�y wolumetrycznej na analityczn�. Nie wp�ywa to na wydajno��, ale mo�e wp�yn�� na jako�� renderowania mg�y. Mniejsze warto�ci powoduj�, �e renderer pokonuje mniejsz� odleg�o�� mi�dzy pr�bkami mg�y, co skutkuje wy�sz� jako�ci� i mniejsz� liczb� artefakt�w.")]
    public MinFloatParameter maxDistanceVolumetric = new MinFloatParameter(64, 10);

    [Tooltip("Mg�a analityczna zast�puje mg�� wolumetryczn� po odleg�o�ci pr�bkowania mg�y wolumetrycznej. Wy��czenie mg�y analitycznej mo�e poprawi� wydajno��.")]
    public BoolParameter analyticFogEnabled = new BoolParameter(false);

    [Tooltip("Maksymalna odleg�o��, kt�ra b�dzie symulowana dla mg�y analitycznej. Nie wp�ywa to na wydajno��, ale mo�e wp�yn�� na wizualn� charakterystyk� sceny.")]
    public MinFloatParameter maxDistanceAnalytic = new MinFloatParameter(5000, 100);

    [Tooltip("W��cza Temporal Anti-Aliasing, kt�ry mo�e poprawi� jako�� mg�y w scenariuszach z g�st� mg��.")]
    public BoolParameter temporalAntiAliasingEnabled = new BoolParameter(false);

    [Tooltip("Okre�la wa�no�� najnowszych klatek integrowania mg�y. Ni�sze warto�ci oznaczaj�, �e wi�cej danych jest u�ywanych z pami�ci podr�cznej danych historycznych.")]
    public ClampedFloatParameter temporalAntiAliasingIntegrationRate = new ClampedFloatParameter(0.03f, 0.01f, 0.99f);

    [Tooltip("Pozwala kontrolowa�, czy maski przeciwmgielne maj� by� dodawane, czy tylko renderowane")]
    public FogMaskBlendModeParameter fogMaskBlendMode = new FogMaskBlendModeParameter(FogMaskBlendMode.Additive);


    // Parametry mg�y
    [Tooltip("G�sto�� mg�y na scenie.")]
    public MinFloatParameter fogDensity = new MinFloatParameter(10, 0);

    [Tooltip("Okre�la kierunek, w kt�rym �wiat�o b�dzie si� rozprasza�. Warto�ci ujemne powoduj� rozproszenie �wiat�a do ty�u. Warto�ci dodatnie powoduj� rozproszenie �wiat�a do przodu.")]
    public ClampedFloatParameter anisotropy = new ClampedFloatParameter(0.2f, -1, 1);

    [Tooltip("Mno�nik, kt�ry wp�ywa na intensywno�� efekt�w �wietlnych mg�y w niezacienionych obszarach. Zwi�kszenie tego powoduje, �e o�wietlenie mg�y staje si� ja�niejsze w o�wietlonych obszarach. Powinien by� u�ywany tylko do efekt�w stylizowanych. [Domy�lnie: 1]")]
    public MinFloatParameter lightIntensity = new MinFloatParameter(1, 0);

    [Tooltip("Mno�nik wp�ywaj�cy na g�sto�� mg�y w zacienionych obszarach. Zwi�kszenie tego powoduje, �e mg�a staje si� g�stsza w zacienionych obszarach. W efekcie powoduje to, �e zacienione obszary staj� si� bardziej widoczne. Nale�y jednak uwa�a�, poniewa� mo�e to r�wnie� spowodowa�, �e zacienione obszary stan� si� bardziej nieprzejrzyste. Powinien by� u�ywany tylko do efekt�w stylizowanych. [Domy�lnie: 1]")]
    public MinFloatParameter shadowIntensity = new MinFloatParameter(1, 0);


    // Geometria
    [Tooltip("Definiuje \"pod�og�\" obj�to�ci mg�y. Opadanie mg�y zaczyna si� dopiero po przekroczeniu tej wysoko�ci przez mg��.")]
    public FloatParameter baseHeight = new FloatParameter(0);

    [Tooltip("Okre�la wysoko��, na kt�rej g�sto�� mg�y zostanie os�abiona do 36% ustawionej g�sto�ci mg�y. Jest to zawsze obliczane na podstawie wysoko�ci bazowej. Na przyk�ad, je�li twoja wysoko�� podstawowa wynosi 10, a rozmiar granicy t�umienia wynosi 15, twoja mg�a b�dzie mia�a pe�n� g�sto�� do wysoko�ci y = 10 jednostek, p�niej zmniejszy si� do 36% pierwotnej g�sto�ci.")]
    public MinFloatParameter attenuationBoundarySize = new MinFloatParameter(10, 1);


    // Stylizacja
    [Tooltip("Tekstura, kt�rej mo�na u�y� do nadania mgle warto�ci emisji i manipulowania kolorem mg�y w o�wietlonych i zacienionych obszarach. Zwykle u�ywany do efekt�w stylizowanych.")]
    public Texture2DParameter colorRamp = new Texture2DParameter(null);

    [Tooltip("Kontroluje stopie� wp�ywu palety kolor�w na podstawowe wyniki fizyczne. Warto�� 0 oznacza, �e zmiana koloru jest ca�kowicie ignorowana. Warto�� 1 oznacza, �e odnosimy si� wy��cznie do palety kolor�w")]
    public ClampedFloatParameter colorRampInfluence = new ClampedFloatParameter(0, 0, 1);


    // Szum
    [Tooltip("Tekstura 3D, kt�ra zostanie u�yta do zdefiniowania intensywno�ci mg�y. Powtarza si� w domenie kafelkowania szumu (tiling). Warto�� 0 oznacza, �e g�sto�� mg�y jest os�abiona do 0. Warto�� 1 oznacza, �e g�sto�� mg�y nie jest os�abiana i odpowiada warto�ci ustawionej w parametrze G�sto�� mg�y.")]
    public Texture3DParameter noiseTexure = new Texture3DParameter(null);

    [Tooltip("Stopniowo ponownie pr�bkuje tekstur� szumu w mniejszych domenach kafelkowych, aby zwi�kszy� poziom szczeg�owo�ci ostatecznej prezentacji mg�y. Wi�cej oktaw jest bardziej kosztowne obliczeniowo, ale zwi�ksza poziom realizmu.")]
    public ClampedIntParameter octaves = new ClampedIntParameter(2, 1, 3);

    [Tooltip("Warto��, o kt�r� cz�stotliwo�� pr�bkowania tekstury b�dzie wzrasta� z ka�d� kolejn� oktaw�.")]
    public ClampedFloatParameter lacunarity = new ClampedFloatParameter(2, 1, 8);

    [Tooltip("Warto��, o kt�r� amplituda tekstury b�dzie si� zmniejsza� z ka�d� kolejn� oktaw�.")]
    public ClampedFloatParameter gain = new ClampedFloatParameter(0.3f, 0, 1);

    [Tooltip("D�ugo�� ka�dego boku sze�cianu, kt�ra opisuje szybko�� powtarzania si� tekstury szumu. Innymi s�owy, skala tekstury szumu w metrach.")]
    public MinFloatParameter noiseTiling = new MinFloatParameter(50, 0);

    [Tooltip("Szybko��, z jak� szum b�dzie przesuwa� si� w czasie. U�yj tego do symulacji wiatru. Mierzona w metrach na sekund�.")]
    public Vector3Parameter noiseWindSpeed = new Vector3Parameter(new Vector3(0, 0, 0));

    [Tooltip("Odwzorowuje tekstur� wej�ciow� szumu z oryginalnego zakresu [0, 1] na nowy zakres zdefiniowany przez [NoiseIntensityMin, NoiseIntensityMax]. Na przyk�ad mapowanie [0,2, 0,8] spowoduje ponowne mapowanie szumu przez obci�cie dowolnych warto�ci poni�ej 0,2 i powy�ej 0,8, a nast�pnie ponowne odwzorowanie pozosta�ego zakresu od 0,2 do 0,8 z powrotem do 0,0 do 1,0 w celu zachowania szczeg��w.")]
    public FloatRangeParameter noiseMap = new FloatRangeParameter(new Vector2(0, 1), 0, 1);


    public bool IsActive()
    {
        if (mode.value != VolumetricFogMode.On || fogDensity.value <= 0)
            return false;

        return true;
    }

    public bool IsTileCompatible() => false;
}
