struct Shape {
	float4x4 transformMatrix;

	float3 size;
	float3 color;

	int shapeType;

	int numTransforms;

	float roughness;
	float metalness;
};

float sdSphere(float3 p) {
	return length(p) - 1;
}

float sdBox(float3 p) {
	float3 q = abs(p) - float3(1, 1, 1);
	return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}

float sdTorus(float3 p, float2 t) {
	float2 q = float2(length(p.xz) - t.x, p.y);
	return length(q) - t.y;
}