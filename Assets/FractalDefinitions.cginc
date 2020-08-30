float sdMandelbulb(float3 pos) {
	float3 z = pos;
	float dr = 1.0;
	float r = 0.0;
	for (int i = 0; i < 15; i++) {
		r = length(z);
		if (r > 2) break;

		// convert to polar coordinates
		float theta = acos(z.z / r);
		float phi = atan2(z.y, z.x);
		dr = pow(r, 8 - 1.0) * 8 * dr + 1.0;

		// scale and rotate the point
		float zr = pow(r, 8);
		theta = theta * 8;
		phi = phi * 8;

		// convert back to cartesian coordinates
		z = zr * float3(sin(theta) * cos(phi), sin(phi) * sin(theta), cos(theta));
		z += pos;
	}
	return 0.5 * log(r) * r / dr;
}