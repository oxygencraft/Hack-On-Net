using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Database
{
    class DatabaseDump
    {
        private static List<string> commands = new List<string>
        {
            "/*!40101 SET @saved_cs_client     = @@character_set_client */",
            "/*!40101 SET character_set_client = utf8 */",
            "DROP TABLE IF EXISTS `accounts`",
            "CREATE TABLE `accounts` (" +
            " `id` int(11) NOT NULL AUTO_INCREMENT," +
            " `username` varchar(64) NOT NULL," +
            " `pass` char(40) DEFAULT NULL," +
            " `mailaddress` varchar(64) DEFAULT NULL," +
            " `homeComputer` int(11) DEFAULT NULL," +
            " PRIMARY KEY (`id`)," +
            " UNIQUE KEY `username` (`username`)," +
            " UNIQUE KEY `mailaddress` (`mailaddress`)" +
            ") ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1",
            //
            // Dumping data for table `accounts`
            //
            "LOCK TABLES `accounts` WRITE",
            "/*!40000 ALTER TABLE `accounts` DISABLE KEYS */",
            "INSERT INTO `accounts` VALUES (1,'root','','root@hnmp.net',1),(2,'test','da39a3ee5e6b4b0d3255bfef95601890afd80709','test@hnmp.net',1)",
            "/*!40000 ALTER TABLE `accounts` ENABLE KEYS */",
            "UNLOCK TABLES",
            "/*!40101 SET character_set_client = @saved_cs_client */",
            "/*!40101 SET @saved_cs_client     = @@character_set_client */",
            "/*!40101 SET character_set_client = utf8 */",
            "DROP TABLE IF EXISTS `computers`",
            "CREATE TABLE `computers` (" +
            " `id` int(10) unsigned NOT NULL AUTO_INCREMENT," +
            " `ip` varchar(15) NOT NULL," +
            " `owner` varchar(64) NOT NULL," +
            " `type` int(11) NOT NULL," +
            " PRIMARY KEY (`id`)," +
            " UNIQUE KEY `ip` (`ip`)" +
            ") ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1",
            //
            // Dumping data for table `computers`
            //
            "LOCK TABLES `computers` WRITE",
            "/*!40000 ALTER TABLE `computers` DISABLE KEYS */",
            "INSERT INTO `computers` VALUES (1,'8.8.8.8',1,4),(2,'2.2.2.2',1,4)",
            "/*!40000 ALTER TABLE `computers` ENABLE KEYS */",
            "UNLOCK TABLES",
            "/*!40101 SET character_set_client = @saved_cs_client */",
            "/*!40101 SET @saved_cs_client     = @@character_set_client */",
            "/*!40101 SET character_set_client = utf8 */",
            "DROP TABLE IF EXISTS `files`",
            "CREATE TABLE `files` (" +
            " `id` int(11) NOT NULL AUTO_INCREMENT," +
            " `name` varchar(255) NOT NULL," +
            " `parentFile` int(11) NOT NULL," +
            " `type` tinyint(4) NOT NULL," +
            " `specialType` int(11) NOT NULL," +
            " `content` text," +
            " `computerId` int(11) NOT NULL," +
            " `groupId` int(11) NOT NULL," +
            " `permissions` int(11) NOT NULL," +
            " `owner` varchar(64) NOT NULL," +
            " PRIMARY KEY (`id`)," +
            " UNIQUE KEY `uniquefiles` (`name`,`parentFile`,`computerId`)" +
            ") ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=latin1",
            "/*!40101 SET character_set_client = @saved_cs_client */",
            //
            // Dumping data for table `files`
            //
            "LOCK TABLES `files` WRITE",
            "/*!40000 ALTER TABLE `files` DISABLE KEYS */",
            "INSERT INTO `files` VALUES " +
            "(1,'daemons',5,1,0,'',1,3,770,'root')," +
            "(2,'autorun',1,0,0,'irc',1,3,770,'root')," +
            "(3,'irc',1,0,1,'IRC',1,3,770,'root')," +
            "(4,'daemons',6,1,0,'',2,3,770,'root')," +
            "(5,'',0,1,0,'',1,3,770,'root')," +
            "(6,'',0,1,0,'',2,3,770,'root')," +
            "(7,'cfg',5,1,0,'',1,3,770,'root')," +
            "(8,'users.cfg',7,0,0," +
            "'root,root=potato\r\n" +
            "user,jaber=potato\r\n'" +
            ",1,3,770,'root')",
            "/*!40000 ALTER TABLE `files` ENABLE KEYS */",
            "UNLOCK TABLES",
        };

        public static List<string> Commands => commands;
    }
}
