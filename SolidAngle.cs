using UnityEngine;

public class SolidAngleCalculator : MonoBehaviour
{
    // Resolution for the image (width and height)
    public int imageWidth = 128;
    public int imageHeight = 64;

    // Texture to visualize the solid angle
    private Texture2D solidAngleTexture;

    // Method to calculate solid angle for a given theta1, theta2, and dPhi
    public float CalculateSolidAngle(float theta1, float theta2, float dPhi)
    {
        return (Mathf.Cos(theta1) - Mathf.Cos(theta2)) * dPhi;
    }

    // Convert pixel (x, y) to spherical coordinates (theta, phi)
    public (float theta1, float theta2, float dPhi) PixelToSpherical(int x, int y)
    {
        // Convert pixel x, y to phi (longitude) and theta (latitude) in radians
        float dPhi = Mathf.Deg2Rad * (360f / imageWidth);
        float thetaMid = Mathf.Deg2Rad * (180f * y / imageHeight);
        float dTheta = Mathf.Deg2Rad * (180f / imageHeight);

        float theta1 = thetaMid - dTheta / 2;
        float theta2 = thetaMid + dTheta / 2;

        return (theta1, theta2, dPhi);
    }

    // Get the solid angle for a given pixel (x, y)
    public float GetPixelSolidAngle(int x, int y)
    {
        // Convert pixel (x, y) to spherical coordinates (theta1, theta2, dPhi)
        var (theta1, theta2, dPhi) = PixelToSpherical(x, y);

        // Calculate and return the solid angle
        return CalculateSolidAngle(theta1, theta2, dPhi);
    }

    void Start()
    {
        // Create a new Texture2D to store the solid angle values
        solidAngleTexture = new Texture2D(imageWidth, imageHeight);

        // Variables to track the min and max solid angles for normalization
        float minSolidAngle = float.MaxValue;
        float maxSolidAngle = float.MinValue;

        // First pass: calculate all solid angles and find min/max
        float[,] solidAngles = new float[imageWidth, imageHeight];
        for (int x = 0; x < imageWidth; x++)
        {
            for (int y = 0; y < imageHeight; y++)
            {
                float solidAngle = GetPixelSolidAngle(x, y);
                solidAngles[x, y] = solidAngle;

                // Track min/max solid angles
                if (solidAngle < minSolidAngle) minSolidAngle = solidAngle;
                if (solidAngle > maxSolidAngle) maxSolidAngle = solidAngle;
            }
        }

        // Second pass: normalize solid angles and map to grayscale
        for (int x = 0; x < imageWidth; x++)
        {
            for (int y = 0; y < imageHeight; y++)
            {
                // Normalize solid angle to range [0, 1]
                float normalizedAngle = (solidAngles[x, y] - minSolidAngle) / (maxSolidAngle - minSolidAngle);

                // Set pixel color as grayscale based on normalized solid angle
                Color grayscale = new Color(normalizedAngle, normalizedAngle, normalizedAngle);
                solidAngleTexture.SetPixel(x, y, grayscale);
            }
        }

        // Apply the texture changes
        solidAngleTexture.Apply();

        // Display the texture on a UI RawImage or apply it to a material
        DisplayTexture();
    }

    // Display the texture on a quad or UI element
    void DisplayTexture()
    {
        // Find or create a GameObject with a Renderer (e.g., a quad) to apply the texture
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.localScale = new Vector3(10, 5, 1); // Scale the quad
        quad.transform.position = new Vector3(0, 0, 5); // Move it forward in the scene

        // Apply the texture to the quad's material
        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.mainTexture = solidAngleTexture;
    }
}
