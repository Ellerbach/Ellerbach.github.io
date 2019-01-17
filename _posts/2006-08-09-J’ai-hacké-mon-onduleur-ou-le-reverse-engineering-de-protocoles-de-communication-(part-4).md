---
layout: post
title:  "J’ai hacké mon onduleur ou le reverse engineering de protocoles de communication (part 4)"
date: 2006-08-09 15:53:00 +0100
---
Me voici donc au quatrième épisode. Pour suivre les épisodes précédents, c’est [ici]({% post_url 2006-08-06-J’ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-1) %}) pour le premier, [ici]({% post_url 2006-08-07-J’ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-2) %}) pour le second et [ici]({% post_url 2006-08-08-J’ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-3) %}) pour le troisième. Pour rappel, j’ai donc réussi à obtenir les informations nécessaires à l’ouverture du port série. J’en ai profité pour donner quelques explications qui permettent avec différentes méthode d’obtenir ces informations. Même si l’exemple est basé sur un port série, cela fonctionne avec à peu près n’importe quel type de protocole. Le principe de base reste le même. 

Je tiens quand même à apporter quelques précisions si vous utilisez un procédé d’acquisition discret tel que la carte son. Dans l’exemple que j’ai donné, je suis parti sur une acquisition à 44,1 KHz (ce qui correspond à ce que la plupart des cartes d’acquisition audio fond en standard). Il ne faut pas oublier le théorème de Nyquist – Shanon qui stipule que la fréquence maximale que l’on peut acquérir est égale à la moitié de la fréquence d’acquisition. Donc si l’on acquière à 44,1 KHz, cela donne donc une fréquence maximale de 22,05 KHz. En effet, il faut au moins 2 points dans une période pour déterminer la fréquence d’un sinus pur. Si l’on utilise une carte son, avec cette fréquence d’acquisition, cela permet de calculer jusqu’à 19 600 baud. Au-delà, il faut donc acquérir avec une fréquence plus importante (si la carte le permet) ou vraiment utiliser un oscilloscope. 

Aller, je gardais une autre solution pour ce post : utiliser un port parallèle qui permet d’acquérir avec une fréquence suffisamment importante pour aller jusqu’à 115 KHz /2 soit 57,5 KHz. Le montage n’est pas plus compliqué et il faut écrire à la main le bout de soft qui permet de faire cela. Quand j’étais étudiant (il y a bien longtemps maintenant…), nous nous amusions à faire des chenillards et autres affichages loufoques à base de port parallèle. Un port très sympathique qui a inspiré la plupart des affichages LCD de façade… 

Bon, revenons-en au code. J’ai promis à [Benjamin](http://www.benjamingauthey.com) (qui est dans mon équipe) que je publierais un peu code. Alors, Benj, chose promise, chose due :-) Je vais donc publier un morceau de code qui permet de se connecter à l’onduleur, de lui envoyer du texte et de récupérer ce qui en revient. Ce code est basé sur un projet (How-to Using the Comm Port) de [GotDotNet](http://www.gotdotnet.com/). Il permet aux utilisateurs du framework 1.0 et 1.1 de bénéficier d’une classe super bien faite pour accéder aux ports séries. Ca me rappelle celle que j’avais dû faire et que j’utilisais à l’époque en C++. De base dans le framework 2.0, il y a tout ce qu’il faut avec la classe SerialPort, elle aussi bien faite. Je suis parti du projet de GotDotNet car je l’ai trouvé très rapidement et qu’il y avait du code pour m’inspirer. Donc, benj, ouvre les yeux, voici mon code : 

```vb
' BtnSendOnduleur est un bouton :-) 

Private Sub BtnSendOnduleur_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnSendOnduleur.Click 
    ' Les Ports, c’est comme les fichiers, dès qu’on s’en sert, faut les try catch. 
    Try 
        m_CommPort.Open(1, 2400, 8, Rs232.DataParity.Parity_None, Rs232.DataStopBit.StopBit_1, 64) 
        ' TxtSendOnduleur est une boîte de texte, texte à envoyer à l’onduleur 
        m_CommPort.Write(Encoding.ASCII.GetBytes(TxtSendOnduleur.Text & Chr(13))) 
        'Une pose quand on ouvre un port, ça fait toujours du bien 
        System.Threading.Thread.Sleep(200) 
        Application.DoEvents() 
        ' Try pour la récupération des données. 
        Try 
        ' Je lis une donnée et toutes celles qui suivent. Quand le retour est -1, c’est timeout, donc, en principe, plus de données à lire 
            While (m_CommPort.Read(1) <> -1) 
                ' txtStatus est une boîte de texte qui permet de mettre le résultat de la lecture du port. 
                txtStatus.Text = txtStatus.Text + Chr(m_CommPort.InputStream(0)) 
            End While 

            m_CommPort.Close() 
            Return 

        Catch exc As Exception 
            ' Rien à lire ou un problème 
            m_CommPort.Close() 
            Return 

        End Try 

    Catch exc As Exception 
        ' Impossible d’ouvrir le port 
        MsgBox("Port pas ouvert.", MsgBoxStyle.OkOnly, Me.Text) 
        Return 
    End Try 
End Sub 
```

Maintenant que tu as vu le code, je vais expliquer à quoi il me sert. Revenons sur l’exercice que j’ai fait avec [PortMon](http://www.sysinternals.com/Utilities/Portmon.html). Voici la suite de ce que j’ai récupéré pas la suite : 

```
28 0.00002172 RupsMon.exe IRP_MJ_WRITE Serial0 SUCCESS Length 2: F. 
29 0.11777290 RupsMon.exe IOCTL_SERIAL_WAIT_ON_MASK Serial0 SUCCESS 
30 0.00445378 RupsMon.exe IRP_MJ_WRITE Serial0 SUCCESS Length 3: Q1. 
31 0.00000703 RupsMon.exe IOCTL_SERIAL_GET_COMMSTATUS Serial0 SUCCESS 
32 0.00000273 RupsMon.exe IOCTL_SERIAL_GET_COMMSTATUS Serial0 SUCCESS 
33 0.00000576 RupsMon.exe IRP_MJ_READ Serial0 SUCCESS Length 22: #230.0 2.2 12.00 50.0. 
34 2.09370086 RupsMon.exe IOCTL_SERIAL_WAIT_ON_MASK Serial0 SUCCESS 
35 0.00000559 RupsMon.exe IOCTL_SERIAL_PURGE Serial0 SUCCESS Purge: TXCLEAR 
36 0.00002896 RupsMon.exe IRP_MJ_WRITE Serial0 SUCCESS Length 3: Q1. 
37 0.00000333 RupsMon.exe IOCTL_SERIAL_WAIT_ON_MASK Serial0 INVALID PARAMETER 
38 0.00000359 RupsMon.exe IOCTL_SERIAL_GET_COMMSTATUS Serial0 SUCCESS 
39 0.00000191 RupsMon.exe IOCTL_SERIAL_GET_COMMSTATUS Serial0 SUCCESS 
40 0.00000388 RupsMon.exe IRP_MJ_READ Serial0 SUCCESS Length 47: (239.0 239.0 239.0 022 50.0 13.9 32.0 00001000. 
41 0.00002742 RupsMon.exe IRP_MJ_WRITE Serial0 SUCCESS Length 2: F. 
42 0.11799550 RupsMon.exe IOCTL_SERIAL_WAIT_ON_MASK Serial0 SUCCESS 
43 0.00000374 RupsMon.exe IOCTL_SERIAL_GET_COMMSTATUS Serial0 SUCCESS 
44 0.00000183 RupsMon.exe IOCTL_SERIAL_GET_COMMSTATUS Serial0 SUCCESS 
45 0.00000380 RupsMon.exe IRP_MJ_READ Serial0 SUCCESS Length 22: #230.0 2.2 12.00 50.0. 
46 0.00011600 RupsMon.exe IRP_MJ_WRITE Serial0 SUCCESS Length 3: Q1. 
47 0.22647997 RupsMon.exe IOCTL_SERIAL_WAIT_ON_MASK Serial0 SUCCESS 
48 0.00000381 RupsMon.exe IOCTL_SERIAL_GET_COMMSTATUS Serial0 SUCCESS 
49 0.00000182 RupsMon.exe IOCTL_SERIAL_GET_COMMSTATUS Serial0 SUCCESS 
50 0.00000407 RupsMon.exe IRP_MJ_READ Serial0 SUCCESS Length 47: (239.0 239.0 239.0 022 50.0 13.9 32.0 00001000. 
```

En gros, quand la commande F ou Q1 est envoyée, il y a un retour. Pour définir ce que le point représente, j’ai fait une acquisition en binaire. Cela donne pour Q1 : IRP_MJ_WRITE Serial0 SUCCESS Length 3: 51 31 0D 

0D = 13 en décimal, ce qui correspond au caractère « entrée ». Très classique dans le cas de communication avec des appareils en port série. D’où dans le code, TxtSendOnduleur.Text & Chr(13) qui permet d’envoyer du texte (F ou Q1 pour faire les premiers tests) avec le caractère « entrée ». Mon programme permet donc d’envoyer une commande, de récupérer le résultat et de l’afficher dans une boîte de texte. A noter que j’ai fait ce code uniquement pour me dérouiller. Inutile dans mon cas, j’aurais pu utiliser le bon vieux terminal Windows qui fait exactement la même chose (en mieux). J’ai écrit ce code, toujours dans l’optique de me dérouiller. De toute façon, ce n’est pas perdu, j’aurais besoin d’en écrire et à peu près le même pour mon application finale. Je reviendrais sur cet excellent outil qu’est le Terminal Windows et qui m’a presque fait oublier ma [VT100](http://en.wikipedia.org/wiki/Vt100) … 

A noter qu’avec VB 2005, pour ouvrir un port, on peut utiliser l’excellente classe My en faisant un Dim MonPort As System.IO.Ports.SerialPort = My.Computer.Ports.OpenSerialPort("COM1", 2400, IO.Ports.Parity.None, 8, IO.Ports.StopBits.One). Ensuite, la classe SerialPort permet d’envoyer et récupérer des données. Je l’utiliserais dans mon application finale. 

Du coup, cette application, m’a permit d’envoyer diverses commandes et de voir la réaction de l’onduleur et le retour qu’il a pu m’en faire. Cela m’a permit de déterminer (quasiment) toutes les commandes disponibles et donc de déterminer quel est le langage entre l’onduleur et le PC. La suite au prochaine numéro…

