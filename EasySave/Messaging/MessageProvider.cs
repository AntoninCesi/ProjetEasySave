using System;
using EasySave.Resources;

namespace EasySave.Messaging
{
    public class MessageProvider
    {
        // Note: Le bool isFrench n'est plus nécessaire ici car 
        // le LanguageManager gère l'état global de la langue.

        public string Resolve(Message message)
        {
            // 1. On transforme le type de l'enum en string pour l'utiliser comme clé
            string key = message.Type.ToString();

            // 2. On récupère la chaîne localisée via l'indexeur du LanguageManager
            string localizedTemplate = LanguageManager.Instance[key];

            // 3. Si la chaîne contient des paramètres (ex: {0}), on les injecte
            try
            {
                if (message.Parameters != null && message.Parameters.Length > 0)
                {
                    return string.Format(localizedTemplate, message.Parameters);
                }
                return localizedTemplate;
            }
            catch (FormatException)
            {
                // En cas de mauvais formatage dans le fichier resx
                return localizedTemplate;
            }
        }
    }
}