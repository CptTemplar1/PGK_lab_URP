using UnityEditor;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public ComputeShader computeShader; // Referencja do Compute Shadera odpowiedzialnego za generowanie szumu
    ComputeBuffer buffer; // Bufor używany do przekazywania danych pomiędzy GPU a CPU
    public Material fogMat; // Materiał używany do renderowania efektu mgły
    public int size = 32, height = 16; // Rozmiar generowanej tekstury

    static Texture3D tex3D; // Referencja do tekstury 3D
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Sprawdza, czy został naciśnięty klawisz Spacji
        {
            CreateTexture(); // Wywołuje funkcję tworzącą teksturę
        }
    }

    public float noiseSize = 1, seed = 0; // Parametry generowania szumu

    [ContextMenu("Generate Noise")] // Pozwala na wywołanie tej funkcji z menu kontekstowego w edytorze Unity
    void CreateTexture()
    {
        if (tex3D == null) // Sprawdza, czy tekstura 3D nie została jeszcze utworzona
        {
            tex3D = new Texture3D(size, height, size, TextureFormat.RFloat, false); // Tworzy nową teksturę 3D
        }

        fogMat.SetTexture("_Noise", tex3D); // Ustawia teksturę 3D jako parametr materiału efektu mgły
        int pixels = size * size * height; // Oblicza ilość pikseli w teksturze
        ComputeBuffer buffer = new ComputeBuffer(pixels, sizeof(float)); // Tworzy nowy bufor danych

        computeShader.SetBuffer(0, "Result", buffer); // Ustawia bufor jako parametr w Compute Shaderze
        computeShader.SetFloat("size", size); // Ustawia rozmiar jako parametr w Compute Shaderze
        computeShader.SetFloat("height", height); // Ustawia wysokość jako parametr w Compute Shaderze
        computeShader.SetFloat("seed", seed); // Ustawia ziarno jako parametr w Compute Shaderze
        computeShader.SetFloat("noiseSize", noiseSize); // Ustawia rozmiar szumu jako parametr w Compute Shaderze
        computeShader.Dispatch(0, size / 8, height / 8, size / 8); // Wywołuje obliczenia w Compute Shaderze

        float[] noise = new float[pixels]; // Tablica przechowująca wartości szumu
        Color[] colors = new Color[pixels]; // Tablica przechowująca kolory
        buffer.GetData(noise); // Pobiera dane z bufora
        buffer.Release(); // Zwalnia bufor

        for (int i = 0; i < pixels; i++) // Przetwarza wartości szumu na kolory
        {
            colors[i] = new Color(noise[i], 0, 0, 0);
        }
        tex3D.SetPixels(colors); // Ustawia kolory na teksturze 3D
        tex3D.Apply(); // Zastosowuje zmiany w teksturze

    }


    [ContextMenu("Save Noise")] // Pozwala na wywołanie tej funkcji z menu kontekstowego w edytorze Unity

    //funkcja zapisująca teksturę do projektu Unity w ścieżce
    void CreateTexture3D()
    {
        AssetDatabase.CreateAsset(tex3D, "Assets/Shaders/FogShader/3DTextureNoise.asset");
    }
}

