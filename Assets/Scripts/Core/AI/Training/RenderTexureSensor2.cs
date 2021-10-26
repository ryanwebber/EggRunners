using UnityEngine;
using System.Collections;
using Unity.MLAgents.Sensors;
using System;

public class RenderTextureSensor2 : ISensor, IDisposable
{
    RenderTexture m_RenderTexture;
    bool m_Grayscale;
    string m_Name;
    private ObservationSpec m_ObservationSpec;
    SensorCompressionType m_CompressionType;
    Texture2D m_Texture;

    /// <summary>
    /// The compression type used by the sensor.
    /// </summary>
    public SensorCompressionType CompressionType
    {
        get { return m_CompressionType; }
        set { m_CompressionType = value; }
    }

    /// <summary>
    /// Initializes the sensor.
    /// </summary>
    /// <param name="renderTexture">The [RenderTexture](https://docs.unity3d.com/ScriptReference/RenderTexture.html)
    /// instance to wrap.</param>
    /// <param name="grayscale">Whether to convert it to grayscale or not.</param>
    /// <param name="name">Name of the sensor.</param>
    /// <param name="compressionType">Compression method for the render texture.</param>
    /// [GameObject]: https://docs.unity3d.com/Manual/GameObjects.html
    public RenderTextureSensor2(
        RenderTexture renderTexture, bool grayscale, string name, SensorCompressionType compressionType)
    {

        Debug.Log($"Got render texture: {renderTexture.width}x{renderTexture.height}");

        m_RenderTexture = renderTexture;
        var width = renderTexture != null ? renderTexture.width : 0;
        var height = renderTexture != null ? renderTexture.height : 0;
        m_Grayscale = grayscale;
        m_Name = name;
        m_ObservationSpec = ObservationSpec.Visual(height, width, grayscale ? 1 : 3);
        m_CompressionType = compressionType;
        m_Texture = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    /// <inheritdoc/>
    public string GetName()
    {
        return m_Name;
    }

    /// <inheritdoc/>
    public ObservationSpec GetObservationSpec()
    {
        return m_ObservationSpec;
    }

    /// <inheritdoc/>
    public byte[] GetCompressedObservation()
    {
        // TODO support more types here, e.g. JPG
        var compressed = m_Texture.EncodeToPNG();
        return compressed;

    }

    /// <inheritdoc/>
    public int Write(ObservationWriter writer)
    {
        var numWritten = writer.WriteTexture(m_Texture, m_Grayscale);
        return numWritten;
    }

    /// <inheritdoc/>
    public void Update() { }

    /// <inheritdoc/>
    public void Reset() { }

    /// <inheritdoc/>
    public CompressionSpec GetCompressionSpec()
    {
        return new CompressionSpec(m_CompressionType);
    }

    /// <inheritdoc/>
    public BuiltInSensorType GetBuiltInSensorType()
    {
        return BuiltInSensorType.RenderTextureSensor;
    }

    /// <summary>
    /// Converts a RenderTexture to a 2D texture.
    /// </summary>
    /// <param name="obsTexture">RenderTexture.</param>
    /// <param name="texture2D">Texture2D to render to.</param>
    public void UpdateObservationAfterFrameRender()
    {
        var prevActiveRt = RenderTexture.active;
        RenderTexture.active = m_RenderTexture;

        m_Texture.ReadPixels(new Rect(0, 0, m_Texture.width, m_Texture.height), 0, 0);
        m_Texture.Apply();

        RenderTexture.active = prevActiveRt;
    }

    /// <summary>
    /// Clean up the owned Texture2D.
    /// </summary>
    public void Dispose()
    {
        if (!ReferenceEquals(null, m_Texture))
        {
            UnityEngine.Object.Destroy(m_Texture);
            m_Texture = null;
        }
    }
}
