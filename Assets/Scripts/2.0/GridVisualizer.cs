using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using UnityEngine.Rendering;

public class GridVisualizer : MonoBehaviour {

	private int width;
	private int height;
	private int[,] grid;
	private Rectangle[,] quads;

	[SerializeField]
	private List<Color> colors = new List<Color>();

	public void UpdateView(int[,] grid)
	{
		this.grid = grid;
		width = grid.GetLength(0);
		height = grid.GetLength(1);
	}

	void OnEnable()
	{
		RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
	}
	void OnDisable()
	{
		RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
	}

	private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		OnPostRender();
	}

	private void OnPostRender()
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (grid[x, y] == 0 && y > 19) continue;
				Draw.Color = colors[grid[x,y]];

				Draw.Rectangle(transform.position + new Vector3(x + 0.5f, y + 0.5f), Vector2.one);
			}
		}
	}

}
