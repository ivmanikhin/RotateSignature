// Decompiled with JetBrains decompiler
// Type: Ascon.Pilot.SDK.GraphicLayerSample.GraphicLayerSample
// Assembly: Ascon.Pilot.SDK.GraphicLayerSample.ext2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 187B3BB9-3768-4B7C-861E-6A56C03BF53E
// Assembly location: D:\Projects\Pilot-ICE\SDK\b396a650-48de-48bb-bf68-8ed251a97fbe\Ascon.Pilot.SDK.GraphicLayerSample.ext2.dll

using Ascon.Pilot.SDK.Menu;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml.Serialization;
using GraphicLayerSample.Properties;

namespace Ascon.Pilot.SDK.GraphicLayerSample
{
      
    [Export(typeof(IMenu<GraphicLayerElementContext>))]
    public class RotateSignature : IMenu<GraphicLayerElementContext>
    {

        private readonly IObjectModifier _modifier;
        private readonly IFileProvider _fileProvider;
        private readonly IPerson _currentPerson;
        private const string RotateSignatureMenuItem = "RotateSignatureMenuItem";


        [ImportingConstructor]
        public RotateSignature(IObjectModifier modifier, IObjectsRepository repository, IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            _modifier = modifier;
            _currentPerson = repository.GetCurrentPerson();
        }


        public void Build(IMenuBuilder builder, GraphicLayerElementContext context)

        {
            //запрос прав на согласование документа:
            // проверка, подписал ли подписант
            if (context.ElementId == ToGuid(_currentPerson.Id)) //пункт меню добавится, если объект является
                                                                //подписью текущего пользователя
            {
               builder.AddItem(RotateSignatureMenuItem, 0)
                      .WithHeader(Resources.RotateSignatureMenuItem)
                      .WithIsEnabled(true); 
            }
        }

        public void OnMenuItemClick(string name, GraphicLayerElementContext context)
        {

            if (name == RotateSignatureMenuItem)
            {
                foreach (IFile file in context.DataObject.Files)
                {
                    if (file.Name.Equals("PILOT_GRAPHIC_LAYER_ELEMENT_" + context.ElementId.ToString()))
                    {
                        var stream = _fileProvider.OpenRead(file);
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(GraphicLayerElement));
                        GraphicLayerElement element = (GraphicLayerElement)xmlSerializer.Deserialize(stream);
                        if (element.Angle != 0)
                        {
                            element.Angle = 0;
                        }
                        else
                        {
                            element.Angle = 270;
                        }
                        IObjectBuilder objectBuilder = _modifier.Edit(context.DataObject);
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream, element);
                            objectBuilder.AddOrReplaceFile(file.Name, memoryStream, file, DateTime.Now, DateTime.Now, DateTime.Now);
                        };
                        _modifier.Apply();
                    }
                }
            }
        }

        public static Guid ToGuid(int value)
        {
            byte[] b = new byte[16];
            BitConverter.GetBytes(value).CopyTo(b, 0);
            return new Guid(b);
        }
    }
}
