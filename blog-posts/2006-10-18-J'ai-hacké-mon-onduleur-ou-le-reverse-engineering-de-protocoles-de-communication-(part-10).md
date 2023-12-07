# 2006-10-18 J'ai hacké mon onduleur ou le reverse engineering de protocoles de communication (part 10)

Me voici déjà au dixième post de ma série. J'ai déjà décrypté [le protocole de communication](./2006-08-15-J'ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-6).md) de mon onduleur. J'ai implémenté toutes les fonctions nécessaires à une [gestion de cet onduleur](./2006-08-17-J'ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-7).md) avec des propriétés qui permettent de lire les données de courant, tension, etc. J'y ai ajouté des [événements](./2006-09-01-J'ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-8).md) qui se déclenchent en cas de panne électrique, de batterie faible, etc. Me voilà donc avec une classe complète prête à être utiliser.

 Comme mon code est destiné à fonctionner notamment sur un serveur, il est obligatoire que ma classe soit gérée dans une application de type service Windows. Un service fonctionne quoi qu'il arrive, quelque soit l'utilisateur connecté ou non. Il lui est possible d'interagir avec le bureau Windows dans certains cas quand un utilisateur est connecté.

 Un service fonctionne sous un compte utilisateur. Cela lui donne donc les droits liés à cet utilisateur. Il existe un utilisateur un peu particulier qui est le compte System. En général, souvent pour se simplifier la vie, la plupart des services fonctionnent avec ce compte.

 Côté démarrage, il est possible d'opter pour 3 solutions :

* Arrêté (bon, ça ne sert pas à grand-chose sauf quand le service est piloté par une autre application, ce qui peut être parfois le cas).  
* Manuel : le service ne démarre que si l'utilisateur le souhaite ou si un service nécessitant ce service démarre (cas d'une dépendance)  
* Automatique : le service se lance automatiquement avec Windows  Voilà pour les généralités. Maintenant, écrire un service en .NET nécessite d'écrire le service en tant que tel mais aussi une classe spécifique qui va permettre d'installer le service. J'y reviendrais plus loin. Comme je l'ai indiqué dans [mon premier post](./2006-08-06-J'ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-1).md), j'ai décidé d'utiliser les [versions Express de Visual Studio](https://www.microsoft.com/france/msdn/vstudio/express/default.mspx), gratuite pour tout le monde, pour faire mon développement et notamment VB Express. Les versions Express permettent de faire du développement .NET et sont déjà très complètes. Elles ont des limitations notamment dans les templates qui permettent de faciliter le développement. Les autres limitations sont liées au débugage. Dans le cas de mon service, écrire un service en VB Express m'a demandé beaucoup de temps. Certainement la partie la plus longue. Le débugage m'a été impossible. J'ai fait le test a posteriori avec [une version Visual Studio Pro](https://www.microsoft.com/france/msdn/vstudio/gamme.mspx) et si je l'avais développer avec cette version, cela m'aurais fait gagner beaucoup de temps.

 Je vais quand même expliquer comment créer un service avec les versions Express.

 Il faut, comme je l'ai indiqué, créer une classe d'installation du service. Cette classe doit s'appeler ProjectInstaller et doit hériter de System.Configuration.Install.Installer

 Voici le code nécessaire à l'installation d'un service :

```vb
 <RunInstaller(True)> Public Class ProjectInstaller 
    Inherits System.Configuration.Install.Installer 

    Public Sub New() 
        MyBase.New() 
        Dim myServiceProcessInstaller As System.ServiceProcess.ServiceProcessInstaller 
        Dim ServiceOnduleurInstaller As System.ServiceProcess.ServiceInstaller 
        myServiceProcessInstaller = New System.ServiceProcess.ServiceProcessInstaller 
        ServiceOnduleurInstaller = New System.ServiceProcess.ServiceInstaller 
        'ServiceProcessInstaller 
        myServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem 
        myServiceProcessInstaller.Password = Nothing 
        myServiceProcessInstaller.Username = Nothing 
        'ServiceOnduleurInstaller 
        ServiceOnduleurInstaller.DisplayName = "Service Onduleur" 
        ServiceOnduleurInstaller.ServiceName = "Service Onduleur" 
        ServiceOnduleurInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic 
        'ProjectInstaller 
        Installers.AddRange(New System.Configuration.Install.Installer() { myServiceProcessInstaller, ServiceOnduleurInstaller}) 
    End Sub 

 End Class 
```

 Comme expliqué dans les généralités des services, il est nécessaire de déterminer sous quel compte et quel va être le type de démarrage du service. C'est là aussi que le nom du service est écrit en dur. C'est le nom qui apparaît dans la console d'administration des services.

 Pour revenir sur le cas du compte System, ce compte est comme toutes les autres, il possède un login et un mot de passe. Cependant, dans le cas de l'initialisation du compte pour une utilisation avec le compte System, il faut spécifier ServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem et ne pas oublier les deux lignes ServiceProcessInstaller.Password = Nothing et ServiceProcessInstaller.Username = Nothing. Si une de ces variables est modifiée, il y a des chances que le lancement du service ne fonctionne pas car les informations de compte stockées pourraient être étonnées.

 Une fois la classe ProjectInstaller d'écrite, il reste le service en tant que tel à écrire. Voici une classe de base qui associée à la classe précédente fonctionne.

```vb
 Public Class ServiceOnduleur 
 Inherits System.ServiceProcess.ServiceBase 

    Public Sub New() 
        MyBase.New() 
        Me.ServiceName = "Service Onduleur" 
    End Sub 

    ' C'est par ici que le service sera initialisé 

    <MTAThread()> Shared Sub Main() 
        Dim ServicesToRun() As System.ServiceProcess.ServiceBase 
        ' Démarrage du service dans le process 
        ServicesToRun = New System.ServiceProcess.ServiceBase() {New ServiceOnduleur} 
        System.ServiceProcess.ServiceBase.Run(ServicesToRun) 
    End Sub 

    Protected Overrides Sub OnStart(ByVal args() As String) 
        ' Initialisation du service pour le démarrage 
    End Sub 

    Protected Overrides Sub OnStop() 
        ' Code qui arrête le service 
    End Sub 
 End Class 
```

 La classe service doit être publique et héritée de la classe System.ServiceProcess.ServiceBase. Elle doit avoir un point d'entrée (ici Shared Main) qui initialise le service. L'initialisation se fait en créant une nouvelle classe ServiceBase et en le démarrant. Il est possible de démarrer plusieurs services dans le même process. Pour cela, il suffit de faire comme suit :

 ServicesToRun = New System.ServiceProcess.ServiceBase () {New Service1, New MySecondUserService}

 Le minimum pour une classe de type service est d'implémenter une méthode OnStart et une autre OnStop. Comme leurs noms l'indiquent, dans le OnStart, il faut initialiser le service pour qu'il démarre et dans le OnStop ce qu'il faut pour qu'il s'arrête.

 Avec cette classe service, la gestion de l'onduleur, l'envoie d'email, j'ai maintenant tout ce qu'il faut pour écrire un service complet qui me permette de gérer mes onduleurs. Stay tune, il y aura peut-être une suite 😊
