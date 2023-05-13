float Unity_RandomRange_float(float2 Seed)
{
    // Generowanie losowej warto�ci na podstawie podanej warto�ci ziarna (Seed)
    // Wykorzystywane s� funkcje sinusa, mno�enie przez sta�e warto�ci oraz funkcja frac, aby otrzyma� warto�� z zakresu [0, 1]
    float randomno = frac(sin(dot(Seed, float2(12.9898, 78.233))) * 43758.5453);
    return randomno - 0.5;
}

SAMPLER(sampler_linear_repeat);

void volumetricFog_float(
    Texture3D NoiseTexture,         // Tekstura 3D zawieraj�ca szum, u�ywana do generowania warto�ci szumu
    Texture2D Noise2DTexture,       // Tekstura 2D zawieraj�ca szum, u�ywana do generowania warto�ci szumu
    float Noise2DScale,             // Skala aplikowana do pozycji na teksturze 2D w celu wygenerowania warto�ci szumu
    float Samples,                  // Liczba pr�bek mg�y, kt�re b�d� generowane wzd�u� widoku
    float MeshDistance,             // Odleg�o��, na jak� pr�bki mg�y s� generowane od siatki obiektu
    float StartingHeight,           // Pocz�tkowa wysoko�� mg�y
    float OverallHeight,            // Ca�kowita wysoko�� mg�y
    float randomness,               // Warto�� losowa u�ywana do wprowadzenia przypadkowo�ci w generowanych pr�bkach
    float3 Size,                    // Skalowanie pozycji pr�bek mg�y
    float Threshold,                // Pr�g warto�ci szumu, powy�ej kt�rego mg�a b�dzie generowana
    float Multiplier,               // Mno�nik wp�ywaj�cy na si�� generowanej mg�y
    float MaxDistance,              // Maksymalna odleg�o��, na jak� pr�bki mg�y mog� si� rozprzestrzenia�
    float3 Position,                // Aktualna pozycja kamery/widza
    float3 View,                    // Wektor kierunku widoku kamery/widza
    out float Fog                   // Wynikowa warto�� mg�y
) {
    Fog = 0;

    // Warunek sprawdzaj�cy, czy pozycja znajduje si� poza zakresem mg�y
    // Je�li pozycja jest powy�ej lub poni�ej zakresu mg�y, a kierunek widoku wskazuje na g�r� lub d�, zwracamy warto��
    if ((Position.y < StartingHeight - OverallHeight && View.y >= 0) || (Position.y > StartingHeight && View.y <= 0)) {
        return;
    }

    float OverallDistance;                                                  // Ca�kowita odleg�o�� mi�dzy pocz�tkiem a ko�cem mg�y
    float yDistanceToStart = StartingHeight - Position.y;                   // Odleg�o�� w osi y od pozycji do pocz�tku mg�y
    float yDistanceToEnd = Position.y - (StartingHeight - OverallHeight);   // Odleg�o�� w osi y od pozycji do ko�ca mg�y
    float distanceToStart;                                                  // Odleg�o�� od pozycji do pocz�tku mg�y
    float distanceToEnd;                                                    // Odleg�o�� od pozycji do ko�ca mg�y
    float maxDistance;                                                      // Maksymalna odleg�o�� pr�bki mg�y


    bool between = Position.y < StartingHeight&& Position.y > StartingHeight - OverallHeight;
    if (View.y > 0) {
        // Powy�ej mg�y
        // Obliczanie odleg�o�ci do pocz�tku i ko�ca mg�y
        distanceToStart = length((yDistanceToStart / View.y) * View);
        distanceToEnd = length((yDistanceToEnd / View.y) * View);
    }
    else {
        // Poni�ej mg�y
        // Obliczanie odleg�o�ci do pocz�tku i ko�ca mg�y
        distanceToStart = length(((yDistanceToEnd) / View.y) * View);
        distanceToEnd = length(((yDistanceToStart) / View.y) * View);
    }

    // Je�li znajdujemy si� mi�dzy pocz�tkiem a ko�cem mg�y, odleg�o�� do pocz�tku jest zerowa
    distanceToStart = between ? 0 : distanceToStart;

    // Obliczanie ca�kowitej odleg�o�ci mg�y
    OverallDistance = abs(distanceToStart - distanceToEnd) * -1;

    // Je�li odleg�o�� do pocz�tku jest mniejsza ni� warto�� MeshDistance, ograniczamy ilo�� pr�bek do warto�ci Samples lub 50 (je�li warto�� Samples jest wi�ksza)
    Samples = distanceToStart < MeshDistance ? min(Samples, 50) : 0;

    // Obliczanie odleg�o�ci mi�dzy pr�bkami mg�y
    float Distance = max(OverallDistance, (distanceToStart - MeshDistance)) / Samples;

    // Generowanie losowej warto�ci na podstawie wsp�rz�dnej xz kierunku widoku (View)
    // Warto�� losowa jest mno�ona przez randomness, Distance/10 oraz sam kierunek widoku (View)
    float random = Unity_RandomRange_float(View.xz);
    float3 randVec = random * randomness * View * (Distance / 10);

    float3 p;             // Pozycja pr�bki mg�y w tr�jwymiarowej przestrzeni
    float yDistance;      // Odleg�o�� w osi y od pozycji pr�bki mg�y do pocz�tku mg�y
    float noise;          // Warto�� szumu dla danej pr�bki mg�y
    float topBottomFade;  // Warto�� fade (przenikania) dla g�rnej i dolnej cz�ci mg�y
    float3 vectorToAdd;   // Wektor dodawany do wektora od pozycji do pocz�tku mg�y

    // Obliczanie wektora od pozycji do pocz�tku mg�y
    float3 vectorToStart = (View * distanceToStart * -1);

    for (int i = 0; i < Samples; i++) {
        // Przerywanie p�tli, je�li warto�� Fog osi�gnie 4.5
        if (Fog >= 4.5) {
            break;
        }

        // Dodawanie wektora pr�bki do wektora od pozycji do pocz�tku mg�y
        vectorToAdd = vectorToStart + View * Distance * i;

        // Sprawdzanie, czy d�ugo�� wektora jest wi�ksza ni� MaxDistance + random
        // Je�li tak, zwi�kszamy warto�� Fog i kontynuujemy p�tl�
        if (length(vectorToAdd) > MaxDistance + random) {
            Fog += 0.02;
            continue;
        }

        // Obliczanie nowej pozycji p na podstawie pozycji i wektora pr�bki
        p = Position + vectorToAdd;

        // Obliczanie odleg�o�ci w osi y od pozycji do pocz�tku mg�y
        yDistance = StartingHeight - p.y;

        // Modyfikacja pozycji p przez losowy wektor randVec i skalowanie przez Size
        p += randVec;
        p *= Size;

        // Pobieranie warto�ci szumu z tr�jwymiarowej tekstury (NoiseTexture) na podstawie przeskalowanej pozycji p
        float n3D = SAMPLE_TEXTURE3D(NoiseTexture, sampler_linear_repeat, p).x;

        // Pobieranie warto�ci szumu z dwuwymiarowej tekstury (Noise2DTexture) na podstawie przeskalowanej pozycji p.xz
        // Otrzyman� warto�� odejmujemy od 0.5
        float n = SAMPLE_TEXTURE2D(Noise2DTexture, sampler_linear_repeat, p.xz * Noise2DScale).x - .5;

        // Sumowanie warto�ci szum�w
        noise = n + n3D;

        // Obliczanie fade dla g�rnej i dolnej cz�ci mg�y na podstawie odleg�o�ci w osi y
        topBottomFade = saturate(yDistance * 1.25) * saturate((OverallHeight - yDistance) * 1.25);

        // Obliczanie warto�ci Fog na podstawie r�nicy pomi�dzy warto�ci� szumu a Threshold, mno�nika Multiplier i fade topBottomFade
        Fog += saturate((noise - Threshold) * Multiplier * topBottomFade);
    }

    // Obliczanie ostatecznej warto�ci Fog na podstawie eksponencjalnej funkc
    Fog = 1 - saturate(exp(-Fog));
}