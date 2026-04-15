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
        gestures.tapDelegate = delegate.map { wrapper in
            TapDelegateWrapper(wrapper)
        }
    }

    @objc public func setLongPressDelegate(_ delegate: HereLongPressDelegate?) {
        gestures.longPressDelegate = delegate.map { wrapper in
            LongPressDelegateWrapper(wrapper)
        }
    }

    @objc public func setDoubleTapDelegate(_ delegate: HereDoubleTapDelegate?) {
        gestures.doubleTapDelegate = delegate.map { wrapper in
            DoubleTapDelegateWrapper(wrapper)
        }
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