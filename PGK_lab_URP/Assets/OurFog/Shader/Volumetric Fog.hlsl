float Unity_RandomRange_float(float2 Seed)
{
    // Generowanie losowej wartoœci na podstawie podanej wartoœci ziarna (Seed)
    // Wykorzystywane s¹ funkcje sinusa, mno¿enie przez sta³e wartoœci oraz funkcja frac, aby otrzymaæ wartoœæ z zakresu [0, 1]
    float randomno = frac(sin(dot(Seed, float2(12.9898, 78.233))) * 43758.5453);
    return randomno - 0.5;
}

SAMPLER(sampler_linear_repeat);

void volumetricFog_float(
    Texture3D NoiseTexture,         // Tekstura 3D zawieraj¹ca szum, u¿ywana do generowania wartoœci szumu
    Texture2D Noise2DTexture,       // Tekstura 2D zawieraj¹ca szum, u¿ywana do generowania wartoœci szumu
    float Noise2DScale,             // Skala aplikowana do pozycji na teksturze 2D w celu wygenerowania wartoœci szumu
    float Samples,                  // Liczba próbek mg³y, które bêd¹ generowane wzd³u¿ widoku
    float MeshDistance,             // Odleg³oœæ, na jak¹ próbki mg³y s¹ generowane od siatki obiektu
    float StartingHeight,           // Pocz¹tkowa wysokoœæ mg³y
    float OverallHeight,            // Ca³kowita wysokoœæ mg³y
    float randomness,               // Wartoœæ losowa u¿ywana do wprowadzenia przypadkowoœci w generowanych próbkach
    float3 Size,                    // Skalowanie pozycji próbek mg³y
    float Threshold,                // Próg wartoœci szumu, powy¿ej którego mg³a bêdzie generowana
    float Multiplier,               // Mno¿nik wp³ywaj¹cy na si³ê generowanej mg³y
    float MaxDistance,              // Maksymalna odleg³oœæ, na jak¹ próbki mg³y mog¹ siê rozprzestrzeniaæ
    float3 Position,                // Aktualna pozycja kamery/widza
    float3 View,                    // Wektor kierunku widoku kamery/widza
    out float Fog                   // Wynikowa wartoœæ mg³y
) {
    Fog = 0;

    // Warunek sprawdzaj¹cy, czy pozycja znajduje siê poza zakresem mg³y
    // Jeœli pozycja jest powy¿ej lub poni¿ej zakresu mg³y, a kierunek widoku wskazuje na górê lub dó³, zwracamy wartoœæ
    if ((Position.y < StartingHeight - OverallHeight && View.y >= 0) || (Position.y > StartingHeight && View.y <= 0)) {
        return;
    }

    float OverallDistance;                                                  // Ca³kowita odleg³oœæ miêdzy pocz¹tkiem a koñcem mg³y
    float yDistanceToStart = StartingHeight - Position.y;                   // Odleg³oœæ w osi y od pozycji do pocz¹tku mg³y
    float yDistanceToEnd = Position.y - (StartingHeight - OverallHeight);   // Odleg³oœæ w osi y od pozycji do koñca mg³y
    float distanceToStart;                                                  // Odleg³oœæ od pozycji do pocz¹tku mg³y
    float distanceToEnd;                                                    // Odleg³oœæ od pozycji do koñca mg³y
    float maxDistance;                                                      // Maksymalna odleg³oœæ próbki mg³y


    bool between = Position.y < StartingHeight&& Position.y > StartingHeight - OverallHeight;
    if (View.y > 0) {
        // Powy¿ej mg³y
        // Obliczanie odleg³oœci do pocz¹tku i koñca mg³y
        distanceToStart = length((yDistanceToStart / View.y) * View);
        distanceToEnd = length((yDistanceToEnd / View.y) * View);
    }
    else {
        // Poni¿ej mg³y
        // Obliczanie odleg³oœci do pocz¹tku i koñca mg³y
        distanceToStart = length(((yDistanceToEnd) / View.y) * View);
        distanceToEnd = length(((yDistanceToStart) / View.y) * View);
    }

    // Jeœli znajdujemy siê miêdzy pocz¹tkiem a koñcem mg³y, odleg³oœæ do pocz¹tku jest zerowa
    distanceToStart = between ? 0 : distanceToStart;

    // Obliczanie ca³kowitej odleg³oœci mg³y
    OverallDistance = abs(distanceToStart - distanceToEnd) * -1;

    // Jeœli odleg³oœæ do pocz¹tku jest mniejsza ni¿ wartoœæ MeshDistance, ograniczamy iloœæ próbek do wartoœci Samples lub 50 (jeœli wartoœæ Samples jest wiêksza)
    Samples = distanceToStart < MeshDistance ? min(Samples, 50) : 0;

    // Obliczanie odleg³oœci miêdzy próbkami mg³y
    float Distance = max(OverallDistance, (distanceToStart - MeshDistance)) / Samples;

    // Generowanie losowej wartoœci na podstawie wspó³rzêdnej xz kierunku widoku (View)
    // Wartoœæ losowa jest mno¿ona przez randomness, Distance/10 oraz sam kierunek widoku (View)
    float random = Unity_RandomRange_float(View.xz);
    float3 randVec = random * randomness * View * (Distance / 10);

    float3 p;             // Pozycja próbki mg³y w trójwymiarowej przestrzeni
    float yDistance;      // Odleg³oœæ w osi y od pozycji próbki mg³y do pocz¹tku mg³y
    float noise;          // Wartoœæ szumu dla danej próbki mg³y
    float topBottomFade;  // Wartoœæ fade (przenikania) dla górnej i dolnej czêœci mg³y
    float3 vectorToAdd;   // Wektor dodawany do wektora od pozycji do pocz¹tku mg³y

    // Obliczanie wektora od pozycji do pocz¹tku mg³y
    float3 vectorToStart = (View * distanceToStart * -1);

    for (int i = 0; i < Samples; i++) {
        // Przerywanie pêtli, jeœli wartoœæ Fog osi¹gnie 4.5
        if (Fog >= 4.5) {
            break;
        }

        // Dodawanie wektora próbki do wektora od pozycji do pocz¹tku mg³y
        vectorToAdd = vectorToStart + View * Distance * i;

        // Sprawdzanie, czy d³ugoœæ wektora jest wiêksza ni¿ MaxDistance + random
        // Jeœli tak, zwiêkszamy wartoœæ Fog i kontynuujemy pêtlê
        if (length(vectorToAdd) > MaxDistance + random) {
            Fog += 0.02;
            continue;
        }

        // Obliczanie nowej pozycji p na podstawie pozycji i wektora próbki
        p = Position + vectorToAdd;

        // Obliczanie odleg³oœci w osi y od pozycji do pocz¹tku mg³y
        yDistance = StartingHeight - p.y;

        // Modyfikacja pozycji p przez losowy wektor randVec i skalowanie przez Size
        p += randVec;
        p *= Size;

        // Pobieranie wartoœci szumu z trójwymiarowej tekstury (NoiseTexture) na podstawie przeskalowanej pozycji p
        float n3D = SAMPLE_TEXTURE3D(NoiseTexture, sampler_linear_repeat, p).x;

        // Pobieranie wartoœci szumu z dwuwymiarowej tekstury (Noise2DTexture) na podstawie przeskalowanej pozycji p.xz
        // Otrzyman¹ wartoœæ odejmujemy od 0.5
        float n = SAMPLE_TEXTURE2D(Noise2DTexture, sampler_linear_repeat, p.xz * Noise2DScale).x - .5;

        // Sumowanie wartoœci szumów
        noise = n + n3D;

        // Obliczanie fade dla górnej i dolnej czêœci mg³y na podstawie odleg³oœci w osi y
        topBottomFade = saturate(yDistance * 1.25) * saturate((OverallHeight - yDistance) * 1.25);

        // Obliczanie wartoœci Fog na podstawie ró¿nicy pomiêdzy wartoœci¹ szumu a Threshold, mno¿nika Multiplier i fade topBottomFade
        Fog += saturate((noise - Threshold) * Multiplier * topBottomFade);
    }

    // Obliczanie ostatecznej wartoœci Fog na podstawie eksponencjalnej funkc
    Fog = 1 - saturate(exp(-Fog));
}