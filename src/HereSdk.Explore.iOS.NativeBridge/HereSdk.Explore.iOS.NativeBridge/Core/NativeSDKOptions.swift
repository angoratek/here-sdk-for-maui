import Foundation
import heresdk

/// ObjC-visible wrapper for HERE SDK options (initialization).
@objc(HereSdkOptions)
public class HereSdkOptions: NSObject {
    @objc public var accessKeyId: String
    @objc public var accessKeySecret: String
    @objc public var cachePath: String?

    @objc public init(accessKeyId: String, accessKeySecret: String, cachePath: String? = nil) {
        self.accessKeyId = accessKeyId
        self.accessKeySecret = accessKeySecret
        self.cachePath = cachePath
        super.init()
    }

    func toSwift() -> SDKOptions {
        let authMode = AuthenticationMode.withKeySecret(accessKeyId: accessKeyId, accessKeySecret: accessKeySecret)
        var options = SDKOptions(authenticationMode: authMode)
        if let cachePath = cachePath {
            options.cachePath = cachePath
        }
        return options
    }
}

/// ObjC-visible wrapper for HERE SDK engine initialization.
@objc(HereSdkEngine)
public class HereSdkEngine: NSObject {
    @objc public static func initialize(_ options: HereSdkOptions) {
        do {
            SDKNativeEngine.sharedInstance = try SDKNativeEngine(options: options.toSwift())
        } catch {
            NSLog("HereSdkEngine: Failed to initialize SDKNativeEngine: \(error)")
        }
    }

    @objc public static func shutdown() {
        SDKNativeEngine.sharedInstance?.dispose()
        SDKNativeEngine.sharedInstance = nil
    }
}