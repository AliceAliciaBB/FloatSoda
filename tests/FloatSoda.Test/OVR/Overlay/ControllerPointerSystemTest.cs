using System.Numerics;
using FloatSoda.OVR;
using FloatSoda.OVR.Math;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.Test.OVR.Overlay;

public class ControllerPointerSystemTest
{
    private const float Tolerance = 1e-5f;

    /// <summary>OpenVR の姿勢(列ベクトル形式の 3x4 行列)を、軸と位置から作る。</summary>
    private static Matrix4x4 Pose(Vector3 x, Vector3 y, Vector3 z, Vector3 position) => new HmdMatrix34_t
    {
        m0 = x.X, m1 = y.X, m2 = z.X, m3 = position.X,
        m4 = x.Y, m5 = y.Y, m6 = z.Y, m7 = position.Y,
        m8 = x.Z, m9 = y.Z, m10 = z.Z, m11 = position.Z,
    }.ToMatrix4x4();

    private static void AssertNear(Vector3 expected, Vector3 actual) =>
        Assert.True(Vector3.Distance(expected, actual) < Tolerance, $"expected {expected}, actual {actual}");

    [Fact]
    public void ComputeRayDirection_補正なし_コントローラーの負のZ軸を返す()
    {
        // コントローラーを Y 軸まわりに 90° 回した姿勢: X → -Z, Z → +X
        var pose = Pose(-Vector3.UnitZ, Vector3.UnitY, Vector3.UnitX, new Vector3(1, 2, 3));

        var direction = ControllerPointerSystem.ComputeRayDirection(pose, Quaternion.Identity);

        AssertNear(-Vector3.UnitX, direction);
    }

    [Fact]
    public void ComputeRayDirection_X軸まわりに負の45度_前方から45度下を向く()
    {
        var pose = Pose(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ, Vector3.Zero);
        var aim = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathF.PI / 4);

        var direction = ControllerPointerSystem.ComputeRayDirection(pose, aim);

        AssertNear(Vector3.Normalize(new Vector3(0, -1, -1)), direction);
    }

    [Fact]
    public void ComputeRayDirection_回転した姿勢に補正_補正もコントローラー基準で回る()
    {
        // コントローラーを Y 軸まわりに 90° 回した姿勢では、「45°下」もそのまま下向きで、前方だけが -X になる。
        var pose = Pose(-Vector3.UnitZ, Vector3.UnitY, Vector3.UnitX, Vector3.Zero);
        var aim = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathF.PI / 4);

        var direction = ControllerPointerSystem.ComputeRayDirection(pose, aim);

        AssertNear(Vector3.Normalize(new Vector3(-1, -1, 0)), direction);
    }
}
