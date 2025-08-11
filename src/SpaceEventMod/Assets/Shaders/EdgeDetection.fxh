// basically the return value tells u which bordering pixels are filled
// 0 = none or isnt an edge
// 1 = right
// 2 = up
// 3 = up, right
// 4 = left
// 5 = left, right
// 6 = left, up
// 7 = left, right, up
// 8 = down
// 9 = down, right
//10 = down, up 
//11 = down, up, right
//12 = down, left
//13 = down, left, right
//14 = down, left, up
//15 = down, left, up, right
float edgeDetection(sampler2D image, float2 coords, float2 pixelSize) 
{
    float edge = 0.;

    edge += step(tex2D(image, coords + float2(pixelSize.x, 0.)).a, 0.); // right
    edge += step(tex2D(image, coords + float2(0., pixelSize.y)).a, 0.) * 2.; // up
    edge += step(tex2D(image, coords + float2(-pixelSize.x, 0.)).a, 0.) * 4.; // left
    edge += step(tex2D(image, coords + float2(0., -pixelSize.y)).a, 0.) * 8.; // down

    return edge * (1. - step(tex2D(image, coords).a, 0.));
}