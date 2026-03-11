using System.Xml.Serialization;

namespace Mercury.Editor.Models.Modules;

/// <summary>
/// Depicts a gpu module that will be installed in the final system.
/// </summary>
public class GpuModuleDescription : ModuleDescription {
    
    /// <summary>
    /// The base address where the framebuffer will be located in virtual memory.
    /// </summary>
    [XmlAttribute("framebufferAddress")]
    public ulong BaseAddress { get; set; }
    
    /// <summary>
    /// The width in pixels of the framebuffer.
    /// </summary>
    [XmlAttribute("width")]
    public uint Width { get; set; }
    
    /// <summary>
    /// The height in pixels of the framebuffer.
    /// </summary>
    [XmlAttribute("height")]
    public uint Height { get; set; }
}