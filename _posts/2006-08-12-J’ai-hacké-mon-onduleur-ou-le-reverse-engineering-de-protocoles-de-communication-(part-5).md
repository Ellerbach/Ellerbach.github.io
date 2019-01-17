---
layout: post
title:  "J’ai hacké mon onduleur ou le reverse engineering de protocoles de communication (part 5)"
date: 2006-08-12 09:28:00 +0100
---
Dans mon [premier post]({% post_url 2006-08-06-J’ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-1) %}), j’indiquais avoir trouvé en quelques minutes comment faire pour envoyer un mail en SMTP. Parce que ce n’est tout de même pas si simple si le serveur SMTP nécessite une authentification et toujours pour faire plaisir à [Benjamin](http://www.benjamingauthey.com) qui trouve que je n’ai pas écrit assez de code, voici ce qu’il faut écrire : 

```vb
'objet mail 
Dim email As New MailMessage("from@mail.fr", "to@mail.fr") 
'le nom du serveur SMTP ou son adresse IP 
Dim MonSmtpMail As New SmtpClient("smtp.mail.fr") 
'sujet du message 
email.Subject = "Test message onduleur" 
'corps du message 
email.Body = "Ceci est un message de test pour l'envoie du mail à travers l'onduleur" 
'indique qu'il ne faut pas utiliser les informations de sécurité de l'utisilateur 
MonSmtpMail.UseDefaultCredentials = False 
'crée un nouveau credential 
MonSmtpMail.Credentials = New Net.NetworkCredential("login", "motdepasse") 
'indique qu'il faut envoyer le mail par le réseau 
MonSmtpMail.DeliveryMethod = SmtpDeliveryMethod.Network 
'gestion d'erreur 
Try 
    MonSmtpMail.Send(email) 
Catch ex As Exception 
    MessageBox.Show(ex.Message) 
End Try 
```

La vraie ruse se trouve dans l’utilisation du NetWorkCredential qui permet de créer des informations de sécurité avec un login, un mot de passe et un domaine (optionnel). 

S’il n’y a pas besoin d’authentification, ou si l’authentification est intégrée, alors les 3 lignes avant le try ne sont pas nécessaires. 

Alors Benj, t’en penses quoi de ce code ? 

La suite au prochain post…

