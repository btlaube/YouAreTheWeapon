// Source: https://drive.google.com/file/d/1W7OiE4m9MqvWoVD40k31sKC-dOZT2ju2/view


using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(CompositeCollider2D))]
public class ShadowCaster2DCreator : MonoBehaviour
{
	[SerializeField]
	private bool selfShadows = true;

	private CompositeCollider2D tilemapCollider;

	static readonly FieldInfo meshField = typeof(ShadowCaster2D).GetField("m_Mesh", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly FieldInfo shapePathHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly MethodInfo generateShadowMeshMethod = typeof(ShadowCaster2D)
									.Assembly
									.GetType("UnityEngine.Rendering.Universal.ShadowUtility")
									.GetMethod("GenerateShadowMesh", BindingFlags.Public | BindingFlags.Static);

	public void Create()
	{
		// Debug.Log($"Can confirm creation: {this}");
		DestroyOldShadowCasters();
		tilemapCollider = GetComponent<CompositeCollider2D>();

		for (int i = 0; i < tilemapCollider.pathCount; i++)
		{
			Vector2[] pathVertices = new Vector2[tilemapCollider.GetPathPointCount(i)];
			tilemapCollider.GetPath(i, pathVertices);
			GameObject shadowCaster = new GameObject("shadow_caster_" + i);
			shadowCaster.transform.parent = gameObject.transform;
			ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();
			shadowCasterComponent.selfShadows = this.selfShadows;

			Vector3[] testPath = new Vector3[pathVertices.Length];
			for (int j = 0; j < pathVertices.Length; j++)
			{
				testPath[j] = pathVertices[j];
			}

			shapePathField.SetValue(shadowCasterComponent, testPath);
			shapePathHashField.SetValue(shadowCasterComponent, Random.Range(int.MinValue, int.MaxValue));
			meshField.SetValue(shadowCasterComponent, new Mesh());
			generateShadowMeshMethod.Invoke(shadowCasterComponent,
			new object[] { meshField.GetValue(shadowCasterComponent), shapePathField.GetValue(shadowCasterComponent) });
		}
		// Added: Center shadow casters
		foreach (Transform shadowCaster in transform)
        {
			// Debug.Log($"Counting off: {shadowCaster}");
            shadowCaster.localPosition = Vector2.zero;
        }
	}
	public void DestroyOldShadowCasters()
	{

		var tempList = transform.Cast<Transform>().ToList();
		foreach (var child in tempList)
		{
			DestroyImmediate(child.gameObject);
		}
	}
}












































































