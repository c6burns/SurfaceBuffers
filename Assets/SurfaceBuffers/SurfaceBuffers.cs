using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Camera))]
public class SurfaceBuffers : MonoBehaviour
{
	[SerializeField] Material material;
	[SerializeField] ComputeShader cs;
	ComputeBuffer vertexBuffer;
	ComputeBuffer vertexBufferOrig;
	const int triangleCount = 1024;
	const int vertexCount = triangleCount * 3;
	const float range = 5.0f;
	const float twidth = 0.1f;
	Material _material;
	float elapsedTime = 0f;

	struct CustomVertex {
		public Vector3 position;
		public Color color;
	};

	void Start()
	{
		_material = new Material(material);
		_material.name += " (clone)";

		CustomVertex[] points = new CustomVertex[vertexCount];
		vertexBuffer = new ComputeBuffer(vertexCount, Marshal.SizeOf(typeof(CustomVertex)), ComputeBufferType.Default);
		vertexBufferOrig = new ComputeBuffer(vertexCount, Marshal.SizeOf(typeof(CustomVertex)), ComputeBufferType.Default);
		
		Random.InitState(0);

		Vector3 p0off = Vector3.zero, p1off = Vector3.zero, p2off = Vector3.zero;
		p0off.x = twidth;
		p1off.y = twidth;
		p2off.x = -twidth;
		Vector3 position = Vector3.zero;
		Color startColor = Color.black;
		int v;
		for (int t = 0; t < triangleCount; t++) {
			position.x = Random.Range(-range, range);
			position.y = Random.Range(-range, range);

			v = t * 3;

			points[v + 0].position = position + p0off;
			points[v + 0].color = startColor;
			points[v + 1].position = position + p1off;
			points[v + 1].color = startColor;
			points[v + 2].position = position + p2off;
			points[v + 2].color = startColor;
		}
		vertexBuffer.SetData(points);
		vertexBufferOrig.SetData(points);
	}

	void OnDestroy()
	{
		vertexBuffer.Release();
		vertexBufferOrig.Release();
		Destroy(_material);
	}

	void Update()
	{
		elapsedTime += Time.deltaTime;

		// just update all uniforms here and use string IDs ... performance doesn't matter
		uint tx, ty, tz;
		int csKID = cs.FindKernel("MovePoints");
		cs.GetKernelThreadGroupSizes(csKID, out tx, out ty, out tz);
		cs.SetBuffer(csKID, "vertexBufferRead", vertexBufferOrig);
		cs.SetBuffer(csKID, "vertexBufferWrite", vertexBuffer);
		cs.SetInt("vertexCount", vertexCount);
		cs.SetFloat("elapsedTime", elapsedTime);
		cs.Dispatch(csKID, vertexCount / (int)tx + 1, 1, 1);
	}

	void OnPostRender()
	{
		// just update all uniforms here and use string IDs ... performance doesn't matter
		_material.SetBuffer("vertexBuffer", vertexBuffer);
		_material.SetMatrix("_LocalToWorld", Matrix4x4.identity);
		_material.SetMatrix("_WorldToLocal", Matrix4x4.identity);
		_material.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, vertexCount, 1);
	}
}