import Foundation
import heresdk

/// ObjC-visible delegate for tap gestures.
@objc(HereTapDelegate)
public protocol HereTapDelegate: AnyObject {
    @objc func onTap(originX: Double, originY: Double)
}

/// ObjC-visible delegate for long press gestures.
/// Includes gesture state (begin/update/end) as an integer: 0=begin, 1=update, 2=end.
@objc(HereLongPressDelegate)
public protocol HereLongPressDelegate: AnyObject {
    @objc func onLongPress(state: Int, originX: Double, originY: Double)
}

/// ObjC-visible delegate for double tap gestures.
@objc(HereDoubleTapDelegate)
public protocol HereDoubleTapDelegate: AnyObject {
    @objc func onDoubleTap(originX: Double, originY: Double)
}

/// ObjC-visible wrapper for Gestures.
@objc(HereGestures)
public class HereGestures: NSObject {
    private let gestures: Gestures

    // The HERE SDK's Gestures delegate properties are `weak`, so the wrapper
    // instances must be retained here or they (and the callback) die as soon
    // as setXxxDelegate returns. The wrappers themselves hold their ObjC
    // delegate weakly — the C# side must keep its delegate handler alive
    // (e.g. in a handler field).
    private var tapWrapper: TapDelegateWrapper?
    private var longPressWrapper: LongPressDelegateWrapper?
    private var doubleTapWrapper: DoubleTapDelegateWrapper?

    /// Non-@objc init — ObjC can't provide a Gestures argument.
    public init(_ gestures: Gestures) {
        self.gestures = gestures
        super.init()
    }

    /// @objc factory — creates from the bridge view which holds the MapView.
    @objc public convenience init(bridgeView: HereMapBridgeView) {
        self.init(bridgeView.swiftMapView!.gestures)
    }

    @objc public func setTapDelegate(_ delegate: HereTapDelegate?) {
        tapWrapper = delegate.map { TapDelegateWrapper($0) }
        gestures.tapDelegate = tapWrapper
    }

    @objc public func setLongPressDelegate(_ delegate: HereLongPressDelegate?) {
        longPressWrapper = delegate.map { LongPressDelegateWrapper($0) }
        gestures.longPressDelegate = longPressWrapper
    }

    @objc public func setDoubleTapDelegate(_ delegate: HereDoubleTapDelegate?) {
        doubleTapWrapper = delegate.map { DoubleTapDelegateWrapper($0) }
        gestures.doubleTapDelegate = doubleTapWrapper
    }

    var swiftGestures: Gestures {
        return gestures
    }
}

// Internal wrapper classes to bridge ObjC delegates to Swift protocols
private class TapDelegateWrapper: NSObject, TapDelegate {
    private weak var delegate: HereTapDelegate?
    init(_ delegate: HereTapDelegate) { self.delegate = delegate }
    func onTap(origin: Point2D) {
        delegate?.onTap(originX: origin.x, originY: origin.y)
    }
}

private class LongPressDelegateWrapper: NSObject, LongPressDelegate {
    private weak var delegate: HereLongPressDelegate?
    init(_ delegate: HereLongPressDelegate) { self.delegate = delegate }
    func onLongPress(state: GestureState, origin: Point2D) {
        delegate?.onLongPress(state: Int(state.rawValue), originX: origin.x, originY: origin.y)
    }
}

private class DoubleTapDelegateWrapper: NSObject, DoubleTapDelegate {
    private weak var delegate: HereDoubleTapDelegate?
    init(_ delegate: HereDoubleTapDelegate) { self.delegate = delegate }
    func onDoubleTap(origin: Point2D) {
        delegate?.onDoubleTap(originX: origin.x, originY: origin.y)
    }
}