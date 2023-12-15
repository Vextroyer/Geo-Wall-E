namespace GSharpCompiler;
///<summary>ethods for Intercept.</summary>

static class Interceptions
{
    public static Element Intercept(Element element1, Element element2)
    {
        List<Element> resultado = new List<Element>();

        if (element1.Type == ElementType.POINT)
        {
            switch (element2.Type)
            {
                case ElementType.LINE:
                    resultado = InterceptPointLine((Element.Point)element1, (Element.Lines)element2, resultado);
                    break;
                case ElementType.RAY:
                    resultado = InterceptPointRay((Element.Point)element1, (Element.Ray)element2, resultado);
                    break;
                case ElementType.SEGMENT:
                    resultado = InterceptPointSegment((Element.Point)element1, (Element.Segment)element2, resultado);
                    break;
                case ElementType.CIRCLE:
                    resultado = InterceptPointCircle((Element.Point)element1, (Element.Circle)element2, resultado);
                    break;
                case ElementType.ARC:
                    resultado = InterceptPointArc((Element.Point)element1, (Element.Arc)element2, resultado);
                    break;
                case ElementType.POINT:
                    resultado = InterceptPointPoint((Element.Point)element1, (Element.Point)element2, resultado);
                    break;
            }

        }
        else if (element1.Type == ElementType.LINE)
        {
            switch (element2.Type)
            {
                case ElementType.CIRCLE:
                    resultado = InterceptLineCircle((Element.Lines)element1, (Element.Circle)element2, resultado);
                    break;
                case ElementType.RAY:
                    resultado = InterceptLineRay((Element.Lines)element1, (Element.Ray)element2, resultado);
                    break;
                case ElementType.LINE:
                    resultado = InterceptLineLine((Element.Lines)element1, (Element.Lines)element2, resultado);
                    break;
                case ElementType.SEGMENT:
                    resultado = InterceptLineSegment((Element.Lines)element1, (Element.Segment)element2, resultado);
                    break;
                case ElementType.POINT:
                    resultado = InterceptPointLine((Element.Point)element2, (Element.Lines)element1, resultado);
                    break;
            }
        }
        else if (element1.Type == ElementType.SEGMENT)
        {
            switch (element2.Type)
            {
                case ElementType.CIRCLE:
                    resultado = InterceptSegmentCircle((Element.Segment)element1, (Element.Circle)element2, resultado);
                    break;
                case ElementType.SEGMENT:
                    resultado = InterceptSegmentSegment((Element.Segment)element1, (Element.Segment)element2, resultado);
                    break;
                case ElementType.LINE:
                    resultado = InterceptLineSegment((Element.Lines)element2, (Element.Segment)element1, resultado);
                    break;
                case ElementType.POINT:
                    resultado = InterceptPointSegment((Element.Point)element2, (Element.Segment)element1, resultado);
                    break;
                case ElementType.RAY:
                    resultado = InterceptSegmentRay((Element.Segment)element1, (Element.Ray)element2, resultado);
                    break;

            }
        }
        else if (element1.Type == ElementType.RAY)
        {
            switch (element2.Type)
            {
                case ElementType.POINT:
                    resultado = InterceptPointRay((Element.Point)element2, (Element.Ray)element1, resultado);
                    break;
                case ElementType.LINE:
                    resultado = InterceptLineRay((Element.Lines)element2, (Element.Ray)element1, resultado);
                    break;
                case ElementType.SEGMENT:
                    resultado = InterceptSegmentRay((Element.Segment)element2, (Element.Ray)element1, resultado);
                    break;
                case ElementType.RAY:
                    resultado = InterceptRayRay((Element.Ray)element2, (Element.Ray)element1, resultado);
                    break;
                case ElementType.CIRCLE:
                    resultado = InterceptRayCircle((Element.Ray)element1, (Element.Circle)element2, resultado);
                    break;
            }
        }
        else if (element1.Type == ElementType.CIRCLE)
        {
            switch (element2.Type)
            {
                case ElementType.POINT:
                    resultado = InterceptPointCircle((Element.Point)element2, (Element.Circle)element1, resultado);
                    break;
                case ElementType.LINE:
                    resultado = InterceptLineCircle((Element.Lines)element2, (Element.Circle)element1, resultado);
                    break;
                case ElementType.SEGMENT:
                    resultado = InterceptSegmentCircle((Element.Segment)element2, (Element.Circle)element1, resultado);
                    break;
                case ElementType.RAY:
                    resultado = InterceptRayCircle((Element.Ray)element2, (Element.Circle)element1, resultado);
                    break;
            }
        }


        if (resultado.Count >= 1 && resultado[0].Type == ElementType.UNDEFINED)
        {
            return Element.UNDEFINED;
        }
        return new Element.Sequence.Listing(resultado);



    }


    ///Point
    public static List<Element> InterceptPointPoint(Element.Point element1, Element.Point element2, List<Element> resultado)
    {
        if (OperationTable.Operate("==", element1, element2) == Element.TRUE)
        {
            resultado.Add(element1);
        }
        return resultado;

    }
    public static List<Element> InterceptPointLine(Element.Point element1, Element.Lines element2, List<Element> resultado)
    {
        float pendiente = CalcularPendiente(element2.p1, element2.p2);
        float n = element2.p1.y.Value - (element2.p1.x.Value * pendiente);
        if (Math.Abs((element1.x.Value * pendiente + n) - element1.y.Value) <= Utils.Epsilon)
        {
            resultado.Add(element1);
        }
        else if (element2.p2.x.Value == element2.p1.x.Value && element1.x.Value == element2.p1.x.Value)
        {
            resultado.Add(element1);
        }
        return resultado;
    }
    public static List<Element> InterceptPointRay(Element.Point element1, Element.Ray element2, List<Element> resultado)
    {
        float pendiente = CalcularPendiente(element2.p1, element2.p2);
        float n = element2.p1.y.Value - (element2.p1.x.Value * pendiente);
        if (Math.Abs((element1.x.Value * pendiente + n) - element1.y.Value) <= Utils.Epsilon)
        {
            if (element2.p2.x.Value < element2.p1.x.Value && ((element2.p2.x.Value >= element1.x.Value) || (element2.p2.x.Value < element1.x.Value && element2.p1.x.Value >= element1.x.Value)))
            {
                resultado.Add(element1);
            }
            else if (element2.p2.x.Value > element2.p1.x.Value && ((element2.p2.x.Value <= element1.x.Value) || (element2.p2.x.Value > element1.x.Value && element2.p1.x.Value <= element1.x.Value)))
            {
                resultado.Add(element1);
            }

        }
        else if (element2.p2.x.Value == element2.p1.x.Value && element1.x.Value == element2.p1.x.Value)
        {
            if (element2.p2.y.Value < element2.p1.y.Value && ((element1.y.Value <= element2.p2.y.Value) || (element1.y.Value > element2.p2.y.Value && element1.y.Value <= element2.p1.y.Value)))
            {
                resultado.Add(element1);
            }
            else if (element2.p2.y.Value > element2.p1.y.Value && ((element1.y.Value >= element2.p2.y.Value) || (element1.y.Value < element2.p2.y.Value && element1.y.Value >= element2.p1.y.Value)))
            {
                resultado.Add(element1);
            }
        }
        return resultado;
    }
    public static List<Element> InterceptPointSegment(Element.Point element1, Element.Segment element2, List<Element> resultado)
    {
        float pendiente = CalcularPendiente(element2.p1, element2.p2);
        float n = element2.p1.y.Value - (element2.p1.x.Value * pendiente);
        if (Math.Abs((element1.x.Value * pendiente + n) - element1.y.Value) <= Utils.Epsilon)
        {
            if (element2.p2.x.Value < element2.p1.x.Value && ((element2.p2.x.Value <= element1.x.Value && element2.p1.x.Value >= element1.x.Value)))
            {
                resultado.Add(element1);
            }
            else if (element2.p2.x.Value > element2.p1.x.Value && ((element2.p2.x.Value > element1.x.Value && element2.p1.x.Value <= element1.x.Value)))
            {
                resultado.Add(element1);
            }

        }
        else if (element2.p2.x.Value == element2.p1.x.Value && element1.x.Value == element2.p1.x.Value)
        {
            if (element2.p2.y.Value < element2.p1.y.Value && ((element1.y.Value >= element2.p2.y.Value && element1.y.Value <= element2.p1.y.Value)))
            {
                resultado.Add(element1);
            }
            else if (element2.p2.y.Value > element2.p1.y.Value && (element1.y.Value <= element2.p2.y.Value && element1.y.Value >= element2.p1.y.Value))
            {
                resultado.Add(element1);
            }
        }
        return resultado;
    }
    public static List<Element> InterceptPointCircle(Element.Point element1, Element.Circle element2, List<Element> resultado)
    {
        if (Math.Abs(DistanciaPuntoPunto(element1, element2.p1) - element2.radius.Value) <= Utils.Epsilon)
        {

            resultado.Add(element1);
        }
        return resultado;
    }
    public static List<Element> InterceptPointArc(Element.Point element1, Element.Arc element2, List<Element> resultado)
    {
        if (Math.Abs(DistanciaPuntoPunto(element1, element2.p1) - element2.radius.Value) <= Utils.Epsilon)
        {

        }
        return resultado;
    }

    ///Segment
    public static List<Element> InterceptLineSegment(Element.Lines element1, Element.Segment element2, List<Element> resultado)
    {
        float pendiente1 = CalcularPendiente(element1.p1, element1.p2);
        float n1 = element1.p1.y.Value - (element1.p1.x.Value * pendiente1);
        float pendiente2 = CalcularPendiente(element2.p1, element2.p2);
        float n2 = element2.p1.y.Value - (element2.p1.x.Value * pendiente2);
        float newx = 0;
        float newy = 0;

        if (pendiente1 == pendiente2)
        {
            resultado.Add(Element.UNDEFINED);

        }
        else if (pendiente1 == float.NegativeInfinity)
        {
            newx = element1.p1.x.Value;
            newy = pendiente2 * newx + n2;
            resultado = InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultado);
            return resultado;
        }
        //   else if (pendiente2 == float.NegativeInfinity)
        // {
        newx = element2.p1.x.Value;
        newy = pendiente1 * newx + n1;
        resultado = InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultado);
        //  }
        return resultado;
    }
    public static List<Element> InterceptLineLine(Element.Lines element1, Element.Lines element2, List<Element> resultado)
    {
        float pendiente1 = CalcularPendiente(element1.p1, element1.p2);
        float n1 = element1.p1.y.Value - (element1.p1.x.Value * pendiente1);
        float pendiente2 = CalcularPendiente(element2.p1, element2.p2);
        float n2 = element2.p1.y.Value - (element2.p1.x.Value * pendiente2);
        float newx = 0;
        float newy = 0;

        if (pendiente1 == pendiente2)
        {
            resultado.Add(Element.UNDEFINED);

        }
        else if (pendiente1 == float.NegativeInfinity)
        {
            newx = element1.p1.x.Value;
            newy = pendiente2 * newx + n2;
            resultado = InterceptPointLine(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultado);
            return resultado;
        }

        newx = element2.p1.x.Value;
        newy = pendiente1 * newx + n1;
        resultado = InterceptPointLine(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultado);

        return resultado;
    }
    public static List<Element> InterceptLineRay(Element.Lines element1, Element.Ray element2, List<Element> resultado)
    {
        float pendiente1 = CalcularPendiente(element1.p1, element1.p2);
        float n1 = element1.p1.y.Value - (element1.p1.x.Value * pendiente1);
        float pendiente2 = CalcularPendiente(element2.p1, element2.p2);
        float n2 = element2.p1.y.Value - (element2.p1.x.Value * pendiente2);
        float newx = 0;
        float newy = 0;

        if (pendiente1 == pendiente2)
        {
            resultado.Add(Element.UNDEFINED);

        }
        else if (pendiente1 == float.NegativeInfinity)
        {
            newx = element1.p1.x.Value;
            newy = pendiente2 * newx + n2;
            resultado = InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultado);
            return resultado;
        }

        newx = element2.p1.x.Value;
        newy = pendiente1 * newx + n1;
        resultado = InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultado);

        return resultado;
    }
    static List<Element> InterceptLineCircle(Element.Lines element1, Element.Circle element2, List<Element> resultado)
    {
        float pendiente = CalcularPendiente(element1.p1, element1.p2);
        float n = element1.p1.y.Value - (element1.p1.x.Value * pendiente);

        float A = 1 + pendiente * pendiente;
        float B = 2 * pendiente * (n - element2.p1.y.Value) - 2 * element2.p1.x.Value;
        float C = element2.p1.x.Value * element2.p1.x.Value + (n - element2.p1.y.Value) * (n - element2.p1.y.Value) - element2.radius.Value * element2.radius.Value;
        float discriminante = B * B - 4 * A * C;
        if (pendiente == float.NegativeInfinity) // Verificar si la pendiente es indefinida (recta vertical)
        {
            float x = element1.p1.x.Value; // Valor de la intersección en el eje x
            float discriminanteVertical = element2.radius.Value * element2.radius.Value - (x - element2.p1.x.Value) * (x - element2.p1.x.Value);
            if (discriminanteVertical >= 0) // Comprobar si la circunferencia intersecta en el eje x
            {
                float y1 = (float)Math.Sqrt(discriminanteVertical) + element2.p1.y.Value;
                float y2 = element2.p1.y.Value - y1;
                resultado.Add(new Element.Point(new Element.String(""), new Element.Number(x), new Element.Number(y1), new Element.String(""), Color.BLACK));
                if (discriminanteVertical > 0) // Manejar el caso de discriminante positivo
                    resultado.Add(new Element.Point(new Element.String(""), new Element.Number(x), new Element.Number(y2), new Element.String(""), Color.BLACK));
            }
        }
        else
        {
            if (discriminante < 0)
            {
                return resultado;
            }
            else if (discriminante == 0)
            {
                float x = -B / (2 * A);
                float y = pendiente * x + n;
                resultado.Add(new Element.Point(new Element.String(""), new Element.Number(x), new Element.Number(y), new Element.String(""), Color.BLACK));
            }
            else
            {
                float x1 = (float)(-B + Math.Sqrt(discriminante)) / (2 * A);
                float y1 = pendiente * x1 + n;
                float x2 = (float)(-B - Math.Sqrt(discriminante)) / (2 * A);
                float y2 = pendiente * x2 + n;
                resultado.Add(new Element.Point(new Element.String(""), new Element.Number(x1), new Element.Number(y2), new Element.String(""), Color.BLACK));
                resultado.Add(new Element.Point(new Element.String(""), new Element.Number(x2), new Element.Number(y2), new Element.String(""), Color.BLACK));
            }
        }
        return resultado;
    }
    // Segment
    public static List<Element> InterceptSegmentSegment(Element.Segment element1, Element.Segment element2, List<Element> resultado)
    {
        List<Element> resultadocopia = new List<Element>(resultado);
        float pendiente1 = CalcularPendiente(element1.p1, element1.p2);
        float n1 = element1.p1.y.Value - (element1.p1.x.Value * pendiente1);
        float pendiente2 = CalcularPendiente(element2.p1, element2.p2);
        float n2 = element2.p1.y.Value - (element2.p1.x.Value * pendiente2);
        float newx = 0;
        float newy = 0;
        List<Element> resultadoaux1;
        List<Element> resultadoaux2;

        if (pendiente1 == pendiente2)
        {
            if (pendiente1 == float.NegativeInfinity)
            {
                if (element2.p1.y.Value < element2.p2.y.Value)
                {
                    if (element1.p1.y.Value < element1.p1.y.Value)
                    {
                        if (element2.p1.y.Value <= element1.p1.y.Value && element2.p2.y.Value >= element1.p2.y.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else
                        {
                            if (element1.p2.y.Value < element2.p2.y.Value && element1.p2.y.Value >= element2.p1.y.Value)
                            {
                                if (element1.p2.y.Value == element2.p1.y.Value)
                                {
                                    resultado.Add(element1.p2);
                                }
                                else
                                {
                                    resultado.Add(Element.UNDEFINED);
                                }
                            }

                        }

                    }
                    else
                    {
                        if (element2.p1.y.Value <= element1.p2.y.Value && element2.p2.y.Value >= element1.p1.y.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else
                        {
                            if (element1.p1.y.Value < element2.p2.y.Value && element1.p1.y.Value >= element2.p1.y.Value)
                            {
                                if (element1.p1.y.Value == element2.p1.y.Value)
                                {
                                    resultado.Add(element1.p1);
                                }
                                else
                                {
                                    resultado.Add(Element.UNDEFINED);
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (element1.p1.y.Value < element1.p1.y.Value)
                    {
                        if (element2.p2.y.Value <= element1.p1.y.Value && element2.p1.y.Value >= element1.p2.y.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else
                        {
                            if (element1.p2.y.Value < element2.p1.y.Value && element1.p2.y.Value >= element2.p2.y.Value)
                            {
                                if (element1.p2.y.Value == element2.p2.y.Value)
                                {
                                    resultado.Add(element1.p2);
                                }
                                else
                                {
                                    resultado.Add(Element.UNDEFINED);
                                }
                            }

                        }

                    }
                    else
                    {
                        if (element2.p2.y.Value <= element1.p2.y.Value && element2.p1.y.Value >= element1.p1.y.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else
                        {
                            if (element1.p1.y.Value < element2.p1.y.Value && element1.p1.y.Value >= element2.p2.y.Value)
                            {
                                if (element1.p1.y.Value == element2.p2.y.Value)
                                {
                                    resultado.Add(element1.p1);
                                }
                                else
                                {
                                    resultado.Add(Element.UNDEFINED);
                                }
                            }

                        }
                    }
                }
                return resultado;
            }
            else
            {
                if (element2.p1.x.Value < element2.p2.x.Value)
                {
                    if (element1.p1.x.Value < element1.p1.x.Value)
                    {
                        if (element2.p1.x.Value <= element1.p1.x.Value && element2.p2.x.Value >= element1.p2.x.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else
                        {
                            if (element1.p2.x.Value < element2.p2.x.Value && element1.p2.x.Value >= element2.p1.x.Value)
                            {
                                if (element1.p2.x.Value == element2.p1.x.Value)
                                {
                                    resultado.Add(element1.p2);
                                }
                                else
                                {
                                    resultado.Add(Element.UNDEFINED);
                                }
                            }

                        }

                    }
                    else
                    {
                        if (element2.p1.x.Value <= element1.p2.x.Value && element2.p2.x.Value >= element1.p1.x.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else
                        {
                            if (element1.p1.x.Value < element2.p2.x.Value && element1.p1.x.Value >= element2.p1.x.Value)
                            {
                                if (element1.p1.x.Value == element2.p1.x.Value)
                                {
                                    resultado.Add(element1.p1);
                                }
                                else
                                {
                                    resultado.Add(Element.UNDEFINED);
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (element1.p1.x.Value < element1.p1.x.Value)
                    {
                        if (element2.p2.x.Value <= element1.p1.x.Value && element2.p1.x.Value >= element1.p2.x.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else
                        {
                            if (element1.p2.x.Value < element2.p1.x.Value && element1.p2.x.Value >= element2.p2.x.Value)
                            {
                                if (element1.p2.x.Value == element2.p2.x.Value)
                                {
                                    resultado.Add(element1.p2);
                                }
                                else
                                {
                                    resultado.Add(Element.UNDEFINED);
                                }
                            }

                        }

                    }
                    else
                    {
                        if (element2.p2.x.Value <= element1.p2.x.Value && element2.p1.x.Value >= element1.p1.x.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else
                        {
                            if (element1.p1.x.Value < element2.p1.x.Value && element1.p1.x.Value >= element2.p2.x.Value)
                            {
                                if (element1.p1.x.Value == element2.p2.x.Value)
                                {
                                    resultado.Add(element1.p1);
                                }
                                else
                                {
                                    resultado.Add(Element.UNDEFINED);
                                }
                            }

                        }
                    }
                }
            }
            return resultado;
        }
        else if (pendiente1 == float.NegativeInfinity)
        {
            newx = element1.p1.x.Value;
            newy = pendiente2 * newx + n2;
            resultadoaux1 = InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element1, resultadocopia);
            resultadocopia = new List<Element>(resultado);
            resultadoaux2 = InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultadocopia);
            if (resultadoaux1.Count == resultadoaux2.Count && resultadoaux1.Count == 1)
            {
                if (resultadoaux1[0] == resultadoaux2[0])
                {
                    resultado = resultadoaux1;

                }

            }
            return resultado;
        }

        newx = element2.p1.x.Value;
        newy = pendiente1 * newx + n1;
        resultadoaux1 = InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element1, resultadocopia);
        resultadocopia = new List<Element>(resultado);
        resultadoaux2 = InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultadocopia);
        if (resultadoaux1.Count == resultadoaux2.Count && resultadoaux1.Count == 1)
        {
            if (resultadoaux1[0] == resultadoaux2[0])
            {
                resultado = resultadoaux1;
            }

        }
        return resultado;
    }
    public static List<Element> InterceptSegmentRay(Element.Segment element1, Element.Ray element2, List<Element> resultado)
    {
        List<Element> resultadocopia = new List<Element>(resultado);
        float pendiente1 = CalcularPendiente(element1.p1, element1.p2);
        float n1 = element1.p1.y.Value - (element1.p1.x.Value * pendiente1);
        float pendiente2 = CalcularPendiente(element2.p1, element2.p2);
        float n2 = element2.p1.y.Value - (element2.p1.x.Value * pendiente2);
        float newx = 0;
        float newy = 0;
        List<Element> resultadoaux1;
        List<Element> resultadoaux2;

        if (pendiente1 == pendiente2)
        {
            if (pendiente1 == float.NegativeInfinity)
            {
                if (element1.p1.y.Value < element1.p2.y.Value)
                {
                    if (element2.p1.y.Value < element2.p2.y.Value)
                    {
                        if (element2.p1.y.Value <= element1.p2.y.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else if (element2.p1.y.Value == element1.p1.y.Value)
                        {
                            resultado.Add(element2.p1);
                        }
                    }
                    else
                    {
                        if (element2.p1.y.Value >= element1.p2.y.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else if (element2.p1.y.Value == element1.p1.y.Value)
                        {
                            resultado.Add(element2.p1);
                        }
                    }
                }
                else
                {
                    if (element2.p1.y.Value < element2.p2.y.Value)
                    {
                        if (element2.p1.y.Value < element1.p1.y.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else if (element2.p1.y.Value == element1.p2.y.Value)
                        {
                            resultado.Add(element2.p1);
                        }
                        if (element2.p1.y.Value <= element1.p1.y.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                    }
                    else
                    {
                        if (element2.p1.y.Value >= element1.p1.y.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else if (element2.p1.y.Value == element1.p2.y.Value)
                        {
                            resultado.Add(element2.p1);
                        }
                    }
                }
            }
            else
            {
                if (element1.p1.x.Value < element1.p2.x.Value)
                {
                    if (element2.p1.x.Value < element2.p2.x.Value)
                    {
                        if (element2.p1.x.Value <= element1.p2.x.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else if (element2.p1.x.Value == element1.p1.x.Value)
                        {
                            resultado.Add(element2.p1);
                        }
                    }
                    else
                    {
                        if (element2.p1.x.Value >= element1.p2.x.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else if (element2.p1.x.Value == element1.p1.x.Value)
                        {
                            resultado.Add(element2.p1);
                        }
                    }
                }
                else
                {
                    if (element2.p1.x.Value < element2.p2.x.Value)
                    {
                        if (element2.p1.x.Value <= element1.p1.x.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else if (element2.p1.x.Value == element1.p2.x.Value)
                        {
                            resultado.Add(element2.p1);
                        }
                    }
                    else
                    {
                        if (element2.p1.x.Value >= element1.p1.x.Value)
                        {
                            resultado.Add(Element.UNDEFINED);
                        }
                        else if (element2.p1.x.Value == element1.p2.x.Value)
                        {
                            resultado.Add(element2.p1);
                        }
                    }

                }
            }
            return resultado;
        }
        else if (pendiente1 == float.NegativeInfinity)
        {
            newx = element1.p1.x.Value;
            newy = pendiente2 * newx + n2;
            resultadoaux1 = InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element1, resultadocopia);
            resultadocopia = new List<Element>(resultado);
            resultadoaux2 = InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultadocopia);
            if (resultadoaux1.Count == resultadoaux2.Count && resultadoaux1.Count == 1)
            {
                if (resultadoaux1[0] == resultadoaux2[0])
                {
                    resultado = resultadoaux1;

                }

            }
            return resultado;
        }

        newx = element2.p1.x.Value;
        newy = pendiente1 * newx + n1;
        resultadoaux1 = InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element1, resultadocopia);
        resultadocopia = new List<Element>(resultado);
        resultadoaux2 = InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultadocopia);
        if (resultadoaux1.Count == resultadoaux2.Count && resultadoaux1.Count == 1)
        {
            if (resultadoaux1[0] == resultadoaux2[0])
            {
                resultado = resultadoaux1;
            }

        }
        return resultado;
    }
    static List<Element> InterceptSegmentCircle(Element.Segment element1, Element.Circle element2, List<Element> resultado)
    {
        List<Element> resultadocopia = new List<Element>(resultado);
        List<Element> resultadoaux1 = new List<Element>();
        List<Element> resultadoaux2 = new List<Element>();
        float pendiente = CalcularPendiente(element1.p1, element1.p2);
        float n = element1.p1.y.Value - (element1.p1.x.Value * pendiente);

        float A = 1 + pendiente * pendiente;
        float B = 2 * pendiente * (n - element2.p1.y.Value) - 2 * element2.p1.x.Value;
        float C = element2.p1.x.Value * element2.p1.x.Value + (n - element2.p1.y.Value) * (n - element2.p1.y.Value) - element2.radius.Value * element2.radius.Value;
        float discriminante = B * B - 4 * A * C;
        if (pendiente == float.NegativeInfinity) // Verificar si la pendiente es indefinida (recta vertical)
        {
            float x = element1.p1.x.Value; // Valor de la intersección en el eje x
            float discriminanteVertical = element2.radius.Value * element2.radius.Value - (x - element2.p1.x.Value) * (x - element2.p1.x.Value);
            if (discriminanteVertical >= 0) // Comprobar si la circunferencia intersecta en el eje x
            {
                float y1 = (float)Math.Sqrt(discriminanteVertical) + element2.p1.y.Value;
                float y2 = element2.p1.y.Value - y1;
                resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x), new Element.Number(y1), new Element.String(""), Color.BLACK));
                if (discriminanteVertical > 0) // Manejar el caso de discriminante positivo
                    resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x), new Element.Number(y2), new Element.String(""), Color.BLACK));
                if (resultadoaux1.Count > 0)
                {
                    foreach (Element.Point element in resultadoaux1)
                    {

                        InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(element.x.Value), new Element.Number(element.y.Value), new Element.String(""), Color.BLACK), element1, resultadoaux2);
                    }
                    if (resultadoaux1.Count == resultadoaux2.Count)
                    {
                        resultado = resultadoaux1;
                    }
                }

            }
            return resultado;
        }
        else
        {
            if (discriminante < 0)
            {
                return resultado;
            }
            else if (discriminante == 0)
            {
                float x = -B / (2 * A);
                float y = pendiente * x + n;
                resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x), new Element.Number(y), new Element.String(""), Color.BLACK));

            }
            else
            {
                float x1 = (float)(-B + Math.Sqrt(discriminante)) / (2 * A);
                float y1 = pendiente * x1 + n;
                float x2 = (float)(-B - Math.Sqrt(discriminante)) / (2 * A);
                float y2 = pendiente * x2 + n;
                resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x1), new Element.Number(y2), new Element.String(""), Color.BLACK));
                resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x2), new Element.Number(y2), new Element.String(""), Color.BLACK));
            }
        }
        if (resultadoaux1.Count > 0)
        {
            foreach (Element.Point element in resultadoaux1)
            {

                InterceptPointSegment(new Element.Point(new Element.String(""), new Element.Number(element.x.Value), new Element.Number(element.y.Value), new Element.String(""), Color.BLACK), element1, resultadoaux2);
            }
            if (resultadoaux1.Count == resultadoaux2.Count)
            {
                resultado = resultadoaux1;
            }
        }
        return resultado;
    }

    //ray
    public static List<Element> InterceptRayRay(Element.Ray element1, Element.Ray element2, List<Element> resultado)
    {
        List<Element> resultadocopia = new List<Element>(resultado);
        float pendiente1 = CalcularPendiente(element1.p1, element1.p2);
        float n1 = element1.p1.y.Value - (element1.p1.x.Value * pendiente1);
        float pendiente2 = CalcularPendiente(element2.p1, element2.p2);
        float n2 = element2.p1.y.Value - (element2.p1.x.Value * pendiente2);
        float newx = 0;
        float newy = 0;
        List<Element> resultadoaux1;
        List<Element> resultadoaux2;

        if (pendiente1 == pendiente2)
        {
            if (pendiente1 == float.NegativeInfinity)
            {
                if ((element1.p1.y.Value < element1.p2.y.Value && element2.p1.y.Value > element2.p2.y.Value))
                {
                    if ((element1.p1.y.Value == element2.p1.y.Value))
                    {
                        resultado.Add(element1.p1);
                    }
                    else if (element1.p1.y.Value < element2.p1.y.Value)
                    {
                        resultado.Add(Element.UNDEFINED);
                    }


                }
                else if ((element1.p1.y.Value > element1.p2.y.Value && element2.p1.y.Value < element2.p2.y.Value))
                {
                    if ((element1.p1.y.Value == element2.p1.y.Value))
                    {
                        resultado.Add(element1.p1);
                    }
                    else
                    {
                        resultado.Add(Element.UNDEFINED);
                    }
                }
                return resultado;
            }
            else
            {
                if ((element1.p1.x.Value < element1.p2.x.Value && element2.p1.x.Value > element2.p2.x.Value))
                {
                    if ((element1.p1.x.Value == element2.p1.x.Value))
                    {
                        resultado.Add(element1.p1);
                    }
                    else if (element1.p1.x.Value < element2.p1.x.Value)
                    {
                        resultado.Add(Element.UNDEFINED);
                    }

                }
                else if ((element1.p1.x.Value > element1.p2.x.Value && element2.p1.x.Value < element2.p2.x.Value))
                {
                    if ((element1.p1.x.Value == element2.p1.x.Value))
                    {
                        resultado.Add(element1.p1);
                    }
                    else
                    {
                        resultado.Add(Element.UNDEFINED);
                    }
                }
                return resultado;
            }
        }
        else if (pendiente1 == float.NegativeInfinity)
        {
            newx = element1.p1.x.Value;
            newy = pendiente2 * newx + n2;
            resultadoaux1 = InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element1, resultadocopia);
            resultadocopia = new List<Element>(resultado);
            resultadoaux2 = InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultadocopia);
            if (resultadoaux1.Count == resultadoaux2.Count && resultadoaux1.Count == 1)
            {
                if (resultadoaux1[0] == resultadoaux2[0])
                {
                    resultado = resultadoaux1;

                }

            }
            return resultado;
        }

        newx = element2.p1.x.Value;
        newy = pendiente1 * newx + n1;
        resultadoaux1 = InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element1, resultadocopia);
        resultadocopia = new List<Element>(resultado);
        resultadoaux2 = InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(newx), new Element.Number(newy), new Element.String(""), Color.BLACK), element2, resultadocopia);
        if (resultadoaux1.Count == resultadoaux2.Count && resultadoaux1.Count == 1)
        {
            if (resultadoaux1[0] == resultadoaux2[0])
            {
                resultado = resultadoaux1;
            }

        }
        return resultado;
    }
    static List<Element> InterceptRayCircle(Element.Ray element1, Element.Circle element2, List<Element> resultado)
    {
        List<Element> resultadocopia = new List<Element>(resultado);
        List<Element> resultadoaux1 = new List<Element>();
        List<Element> resultadoaux2 = new List<Element>();
        float pendiente = CalcularPendiente(element1.p1, element1.p2);
        float n = element1.p1.y.Value - (element1.p1.x.Value * pendiente);

        float A = 1 + pendiente * pendiente;
        float B = 2 * pendiente * (n - element2.p1.y.Value) - 2 * element2.p1.x.Value;
        float C = element2.p1.x.Value * element2.p1.x.Value + (n - element2.p1.y.Value) * (n - element2.p1.y.Value) - element2.radius.Value * element2.radius.Value;
        float discriminante = B * B - 4 * A * C;
        if (pendiente == float.NegativeInfinity) // Verificar si la pendiente es indefinida (recta vertical)
        {
            float x = element1.p1.x.Value; // Valor de la intersección en el eje x
            float discriminanteVertical = element2.radius.Value * element2.radius.Value - (x - element2.p1.x.Value) * (x - element2.p1.x.Value);
            if (discriminanteVertical >= 0) // Comprobar si la circunferencia intersecta en el eje x
            {
                float y1 = (float)Math.Sqrt(discriminanteVertical) + element2.p1.y.Value;
                float y2 = element2.p1.y.Value - y1;
                resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x), new Element.Number(y1), new Element.String(""), Color.BLACK));
                if (discriminanteVertical > 0) // Manejar el caso de discriminante positivo
                    resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x), new Element.Number(y2), new Element.String(""), Color.BLACK));
                if (resultadoaux1.Count > 0)
                {
                    foreach (Element.Point element in resultadoaux1)
                    {

                        InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(element.x.Value), new Element.Number(element.y.Value), new Element.String(""), Color.BLACK), element1, resultadoaux2);
                    }
                    if (resultadoaux1.Count == resultadoaux2.Count)
                    {
                        resultado = resultadoaux1;
                    }
                }

            }
            return resultado;
        }
        else
        {
            if (discriminante < 0)
            {
                return resultado;
            }
            else if (discriminante == 0)
            {
                float x = -B / (2 * A);
                float y = pendiente * x + n;
                resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x), new Element.Number(y), new Element.String(""), Color.BLACK));

            }
            else
            {
                float x1 = (float)(-B + Math.Sqrt(discriminante)) / (2 * A);
                float y1 = pendiente * x1 + n;
                float x2 = (float)(-B - Math.Sqrt(discriminante)) / (2 * A);
                float y2 = pendiente * x2 + n;
                resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x1), new Element.Number(y2), new Element.String(""), Color.BLACK));
                resultadoaux1.Add(new Element.Point(new Element.String(""), new Element.Number(x2), new Element.Number(y2), new Element.String(""), Color.BLACK));
            }
        }
        if (resultadoaux1.Count > 0)
        {
            foreach (Element.Point element in resultadoaux1)
            {

                InterceptPointRay(new Element.Point(new Element.String(""), new Element.Number(element.x.Value), new Element.Number(element.y.Value), new Element.String(""), Color.BLACK), element1, resultadoaux2);
            }
            if (resultadoaux1.Count == resultadoaux2.Count)
            {
                resultado = resultadoaux1;
            }
        }
        return resultado;
    }


    //auxiliar
    static float CalcularPendiente(Element.Point p1, Element.Point p2)
    {
        if (p2.x.Value - p1.x.Value == 0)
        {
            return float.NegativeInfinity;
        }
        return (p2.y.Value - p1.y.Value) / (p2.x.Value - p1.x.Value);
    }
    static float DistanciaPuntoPunto(Element.Point p1, Element.Point p2)
    {
        float distancia = (float)Math.Sqrt(Math.Pow(p2.x.Value - p1.x.Value, 2) + Math.Pow(p2.y.Value - p1.y.Value, 2));
        return distancia;
    }
}