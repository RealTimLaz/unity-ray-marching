struct AdvancedTransform {
	int transformType;
	float scale;
};

float3 mod(float3 x, float3 y) {
	return x - y * floor(x / y);
}

float3 TranslateRotate(float3 p, float4x4 mat) {
	float4 q = float4(p, 1);
	return mul(mat, q);
}

float3 ScaleBefore(float3 p, float3 scale) {
	return float3(p.x / scale.x, p.y / scale.y, p.z / scale.z);
}

float ScaleAfter(float dst, float3 scale) {
	return dst * min(scale.x, min(scale.y, scale.z));
}

float3 Twist(float3 p, float k) {
	float c = cos(k * p.y);
	float s = sin(k * p.y);

	float2x2 m = { c, s, -s, c};

	float3 q = float3(mul(m, p.xz), p.y);

	return q;
}

float3 Bend(float3 p, float k) {
	float c = cos(k * p.x);
	float s = sin(k * p.x);

	float2x2 m = { c, s, -s, c };

	float3 q = float3(mul(m, p.xy), p.z);

	return q;
}

float3 Repeat(float3 p, float k) {
	float3 q = mod(p, k) - 0.5 * k;

	return float3(q.x, p.y, q.z);
}