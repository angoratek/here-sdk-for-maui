import Foundation
import UIKit
import heresdk

/// ObjC-visible wrapper for MapMarker.
@objc(HereMapMarker)
public class HereMapMarker: NSObject {
    @objc public var latitude: Double
    @objc public var longitude: Double
    @objc public var imageName: String?
    /// Normalized image anchor (0..1) — which point of the image sits on the
    /// coordinate. Defaults to the image center; the default POI pin overrides
    /// this to (0.5, 1.0) so the pin tip points at the coordinate.
    @objc public var anchorU: Double = 0.5
    @objc public var anchorV: Double = 0.5
    private var _swiftMarker: MapMarker?

    @objc public init(latitude: Double, longitude: Double) {
        self.latitude = latitude
        self.longitude = longitude
        super.init()
    }

    /// Create with image name (for MapScene-based add/remove).
    @objc public init(latitude: Double, longitude: Double, imageName: String) {
        self.latitude = latitude
        self.longitude = longitude
        self.imageName = imageName
        super.init()
    }

    /// Creates a MapImage from the imageName (if provided), used by MapScene.addMapMarker.
    func createMapImage() -> MapImage? {
        guard let imageName = imageName else {
            anchorU = 0.5
            anchorV = 1.0
            return HereMapMarker.defaultPinImage()
        }
        // "hereglyph:<codepoint>:<RRGGBB>" is a wrapper-internal convention
        // (see MapService.iOS.AddMapMarker): draw a tinted teardrop pin with
        // a Material Icons glyph so markers can be distinct per item type.
        if imageName.hasPrefix("hereglyph:") {
            anchorU = 0.5
            anchorV = 1.0
            return HereMapMarker.glyphPinImage(spec: imageName)
        }
        // Look in the framework bundle first, then the main app bundle.
        let frameworkBundle = Bundle(for: HereMapMarker.self)
        do {
            return try MapImage(named: imageName, width: 32, height: 32, in: frameworkBundle)
        } catch {
            NSLog("REFAPP_DIAG: HereMapMarker image '%@' not found in framework bundle, trying main bundle", imageName)
        }
        do {
            return try MapImage(named: imageName, width: 32, height: 32, in: nil)
        } catch {
            NSLog("REFAPP_DIAG: HereMapMarker image '%@' not found in main bundle either: %@", imageName, String(describing: error))
        }
        return nil
    }

    func setSwiftMarker(_ marker: MapMarker) {
        self._swiftMarker = marker
    }

    var swiftMarker: MapMarker? {
        return _swiftMarker
    }

    /// Draws a default POI pin (teardrop balloon, white border, inner dot) so
    /// that markers added without an image are still visible on the map.
    /// The tip of the pin is the image bottom-center (see anchorU/anchorV).
    private static func defaultPinImage() -> MapImage? {
        let width: CGFloat = 36
        let height: CGFloat = 48
        let format = UIGraphicsImageRendererFormat()
        format.scale = 3
        let renderer = UIGraphicsImageRenderer(
            size: CGSize(width: width, height: height),
            format: format)
        let image = renderer.image { context in
            let cg = context.cgContext
            let headCenter = CGPoint(x: width / 2, y: 16)
            let headRadius: CGFloat = 14
            let tip = CGPoint(x: width / 2, y: height - 1)

            // Outer white silhouette: circle + tail, unioned.
            let silhouette = CGMutablePath()
            silhouette.addArc(center: headCenter, radius: headRadius, startAngle: 0, endAngle: .pi * 2, clockwise: false)
            silhouette.move(to: CGPoint(x: headCenter.x - 9, y: headCenter.y + 9))
            silhouette.addLine(to: tip)
            silhouette.addLine(to: CGPoint(x: headCenter.x + 9, y: headCenter.y + 9))
            silhouette.closeSubpath()
            cg.addPath(silhouette)
            UIColor.white.setFill()
            cg.fillPath()

            // Inner colored silhouette (same tip → white border tapers to the tip).
            let inner = CGMutablePath()
            inner.addArc(center: headCenter, radius: headRadius - 3, startAngle: 0, endAngle: .pi * 2, clockwise: false)
            inner.move(to: CGPoint(x: headCenter.x - 6.5, y: headCenter.y + 8))
            inner.addLine(to: tip)
            inner.addLine(to: CGPoint(x: headCenter.x + 6.5, y: headCenter.y + 8))
            inner.closeSubpath()
            cg.addPath(inner)
            // Coral #FF385C — the brand accent, same color as the Android pin.
            UIColor(red: 1.0, green: 0.22, blue: 0.36, alpha: 1).setFill()
            cg.fillPath()

            // White inner dot.
            UIColor.white.setFill()
            cg.addArc(center: headCenter, radius: 4.5, startAngle: 0, endAngle: .pi * 2, clockwise: false)
            cg.fillPath()
        }
        do {
            return try MapImage(from: image)
        } catch {
            NSLog("REFAPP_DIAG: HereMapMarker default image creation failed: %@", String(describing: error))
            return nil
        }
    }

    /// Parses a "hereglyph:<decimal-codepoint>:<RRGGBB>" spec and draws the
    /// tinted glyph pin. Falls back to the coral default pin on malformed
    /// input so a marker is never silently invisible.
    private static func glyphPinImage(spec: String) -> MapImage? {
        let parts = spec.split(separator: ":")
        guard parts.count == 3, let codepoint = Int(parts[1]), codepoint > 0,
              let scalar = Unicode.Scalar(codepoint),
              let rgb = UInt32(parts[2], radix: 16), rgb <= 0xFFFFFF else {
            NSLog("REFAPP_DIAG: HereMapMarker malformed glyph spec '%@', using default pin", spec)
            return defaultPinImage()
        }
        let color = UIColor(
            red: CGFloat((rgb >> 16) & 0xFF) / 255.0,
            green: CGFloat((rgb >> 8) & 0xFF) / 255.0,
            blue: CGFloat(rgb & 0xFF) / 255.0,
            alpha: 1)
        return glyphPinImage(glyph: String(Character(scalar)), color: color)
    }

    /// Draws the default pin silhouette, tinted, with a white Material Icons
    /// glyph in the head instead of the plain white dot. Mirrors the Android
    /// TryRenderGlyphPin so both platforms render matching pins.
    private static func glyphPinImage(glyph: String, color: UIColor) -> MapImage? {
        let width: CGFloat = 44
        let height: CGFloat = 56
        let format = UIGraphicsImageRendererFormat()
        format.scale = 3
        let renderer = UIGraphicsImageRenderer(
            size: CGSize(width: width, height: height),
            format: format)
        let image = renderer.image { context in
            let cg = context.cgContext
            let headCenter = CGPoint(x: width / 2, y: height * 0.40)
            let headRadius: CGFloat = width / 2 - 3
            let tip = CGPoint(x: width / 2, y: height - 1)

            // Outer white silhouette: circle + tail, unioned.
            let silhouette = CGMutablePath()
            silhouette.addArc(center: headCenter, radius: headRadius + 2.5, startAngle: 0, endAngle: .pi * 2, clockwise: false)
            silhouette.move(to: CGPoint(x: headCenter.x - (headRadius + 2.5) * 0.62, y: headCenter.y + headRadius * 0.70))
            silhouette.addLine(to: tip)
            silhouette.addLine(to: CGPoint(x: headCenter.x + (headRadius + 2.5) * 0.62, y: headCenter.y + headRadius * 0.70))
            silhouette.closeSubpath()
            cg.addPath(silhouette)
            UIColor.white.setFill()
            cg.fillPath()

            // Inner tinted silhouette (same tip → white border tapers to the tip).
            let inner = CGMutablePath()
            inner.addArc(center: headCenter, radius: headRadius, startAngle: 0, endAngle: .pi * 2, clockwise: false)
            inner.move(to: CGPoint(x: headCenter.x - headRadius * 0.55, y: headCenter.y + headRadius * 0.70))
            inner.addLine(to: tip)
            inner.addLine(to: CGPoint(x: headCenter.x + headRadius * 0.55, y: headCenter.y + headRadius * 0.70))
            inner.closeSubpath()
            cg.addPath(inner)
            color.setFill()
            cg.fillPath()

            // White glyph, vertically centered in the head. The MaterialIcons
            // font ships with the MAUI app (UIAppFonts), so UIFont can load it.
            let fontSize: CGFloat = 15
            if let font = materialIconFont(ofSize: fontSize) {
                let attributes: [NSAttributedString.Key: Any] = [
                    .font: font,
                    .foregroundColor: UIColor.white
                ]
                let text = NSAttributedString(string: glyph, attributes: attributes)
                let size = text.size()
                text.draw(at: CGPoint(x: headCenter.x - size.width / 2,
                                      y: headCenter.y - size.height / 2))
            } else {
                NSLog("REFAPP_DIAG: HereMapMarker MaterialIcons font unavailable, drawing white dot")
                UIColor.white.setFill()
                cg.addArc(center: headCenter, radius: 4.5, startAngle: 0, endAngle: .pi * 2, clockwise: false)
                cg.fillPath()
            }
        }
        do {
            return try MapImage(from: image)
        } catch {
            NSLog("REFAPP_DIAG: HereMapMarker glyph image creation failed: %@", String(describing: error))
            return defaultPinImage()
        }
    }

    private static func materialIconFont(ofSize size: CGFloat) -> UIFont? {
        for name in ["MaterialIcons-Regular", "MaterialIcons", "Material Icons"] {
            if let font = UIFont(name: name, size: size) {
                return font
            }
        }
        return nil
    }
}