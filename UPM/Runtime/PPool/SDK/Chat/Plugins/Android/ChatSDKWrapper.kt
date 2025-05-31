package com.ppool.chat

import android.util.Log
import androidx.annotation.Keep
import com.ppool.chat.ChatConfig
import com.ppool.chat.ChatCredential
import com.ppool.chat.ChatEvent
import com.ppool.chat.ChatMessage
import com.ppool.chat.ChatRoomEvent
import com.ppool.chat.ChatRoomOption
import com.ppool.chat.ChatRoomSession
import com.ppool.chat.ChatSDK
import com.ppool.chat.FormattedMessage
import com.unity3d.player.UnityPlayer
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.asCoroutineDispatcher
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.json.JSONObject
import org.json.JSONArray
import java.util.concurrent.Executors
import kotlin.onFailure

@Keep
object ChatSDKWrapper {

    private const val objectName = "PPoolSDK"
    private const val CallMessage = "CallMessage"
    private const val NotifyMessage = "NotifyMessage"

    private const val IdentifierKey = "identifier"
    private const val ValueKey = "value"
    private const val ResultKey = "result"
    private const val ErrorKey = "error"

    private val roomSessionMap = mutableMapOf<String, ChatRoomSession?>()
    private var registerChatRoomEventMap = mutableMapOf<String, Job?>()
    private var registerChatEvent : Job? = null
    private var registerConnectionStatus : Job? = null

    private val realmExecutor = Executors.newSingleThreadExecutor()
    private val realmDispatcher = realmExecutor.asCoroutineDispatcher()

    @JvmStatic
    fun Initialize(environment: Int, target: String)
    {
        val config: ChatConfig;

        when(environment) {
            0 -> config = ChatConfig.dev(target)
            1 -> config = ChatConfig.qa(target)
            2 -> config = ChatConfig.qa2(target)
            3 -> config = ChatConfig.sandbox(target)
            4 -> config = ChatConfig.live(target)
            else -> config = ChatConfig.dev(target)
        }

        val context = UnityPlayer.currentActivity.applicationContext
        ChatSDK.initialize(context, config)
        println("@@@ ChatSDK.initialize ${environment} ${target}")
    }

    @JvmStatic
    fun Connect(userId: String, token: String, identifier: String)
    {
        CoroutineScope(realmDispatcher).launch {
            val credential = ChatCredential(token = token, userId = userId)
            val result = ChatSDK.connect(credential)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)
            val valueJSONObject = JSONObject()
            valueJSONObject.put(ResultKey, result.isSuccess)

            result
                .onSuccess {
                }
                .onFailure { error ->
                    val errorJson = getErrorJson(error)
                    if (errorJson != null) {
                        valueJSONObject.put(ErrorKey, errorJson)
                    }
                }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.Connect params: $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun Disconnect()
    {
        ChatSDK.disconnect()
        println("@@@ [Native] ChatSDK.Disconnect")
    }

    @JvmStatic
    fun Logout()
    {
        ChatSDK.logout()
        println("@@@ [Native] ChatSDK.Logout")
    }

    @JvmStatic
    fun FetchRooms(identifier: String)
    {
        CoroutineScope(realmDispatcher).launch {
            val result = ChatSDK.fetchRooms()

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()
            valueJSONObject.put(ResultKey, result.isSuccess)

            val jsonArray = JSONArray()

            result
                .onSuccess { rooms ->
                    rooms.forEach{
                        jsonArray.put(it.toJSONObject())
                    }
                    valueJSONObject.put("rooms", jsonArray)
                }
                .onFailure { error ->
                    val errorJson = getErrorJson(error)
                    if (errorJson != null) {
                        valueJSONObject.put(ErrorKey, errorJson)
                    }
                }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.FetchRooms : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun FetchRoom(id: String, identifier: String)
    {
        CoroutineScope(realmDispatcher).launch {
            val result = ChatSDK.fetchRoom(id)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()
            valueJSONObject.put(ResultKey, result.isSuccess)

            result
                .onSuccess { room ->
                    valueJSONObject.put("room", room.toJSONObject())
                }
                .onFailure { error ->
                    val errorJson = getErrorJson(error)
                    if (errorJson != null) {
                        valueJSONObject.put(ErrorKey, errorJson)
                    }
                }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.FetchRoom : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun CreateRoom(chatRoomOptionJson: String, identifier: String)
    {
        println("@@@ [Native] ChatSDK.chatRoomOptionJson : $chatRoomOptionJson")
        val optionJsonObject = JSONObject(chatRoomOptionJson)
        val option = ChatRoomOption(optionJsonObject)

        println("@@@ [Native] option : ${option.userIds} ${option.title} ${option.profileUrl} ${option.type}")

        CoroutineScope(realmDispatcher).launch {
            val result = ChatSDK.createRoom(option)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()
            valueJSONObject.put(ResultKey, result.isSuccess)

            result
                .onSuccess { room ->
                    valueJSONObject.put("room", room.toJSONObject())
                }
                .onFailure { error ->
                    val errorJson = getErrorJson(error)
                    if (errorJson != null) {
                        valueJSONObject.put(ErrorKey, errorJson)
                    }
                }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.CreateRoom : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun Invite(roomId: String, userIdsJson: String, identifier: String)
    {
        println("@@@ [Native] ChatSDK.Invite roomId: $roomId userIdsJson: $userIdsJson")
        val userIds = parseJsonToList(userIdsJson)

        CoroutineScope(realmDispatcher).launch {
            val result = ChatSDK.invite(roomId, userIds)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()
            valueJSONObject.put(ResultKey, result.isSuccess)

            result
                .onSuccess {
                }
                .onFailure { error ->
                    val errorJson = getErrorJson(error)
                    if (errorJson != null) {
                        valueJSONObject.put(ErrorKey, errorJson)
                    }
                }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.Invite : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    fun parseJsonToList(json: String): List<String> {
        val jsonArray = JSONArray(json)
        val list = mutableListOf<String>()
        for (i in 0 until jsonArray.length()) {
            list.add(jsonArray.getString(i))
        }
        return list
    }

    @JvmStatic
    fun SetAlarm(roomId: String, alarm: Boolean, identifier: String)
    {
        CoroutineScope(realmDispatcher).launch {
            val result = ChatSDK.setAlarm(roomId, alarm)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()
            valueJSONObject.put(ResultKey, result.isSuccess)

            result
                .onSuccess {
                }
                .onFailure { error ->
                    val errorJson = getErrorJson(error)
                    if (errorJson != null) {
                        valueJSONObject.put(ErrorKey, errorJson)
                    }
                }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.SetAlarm : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun Enter(roomId: String, identifier: String)
    {
        CoroutineScope(realmDispatcher).launch {
            val result = ChatSDK.enter(roomId)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()
            valueJSONObject.put(ResultKey, result.isSuccess)

            result
                .onSuccess {
                    val chatRoom = result.getOrNull()
                    roomSessionMap[roomId] = chatRoom;

                    chatRoom?.let { room ->
                        jsonObject.put(ValueKey, valueJSONObject)

                        withContext(Dispatchers.Main) {
                            val params = jsonObject.toString()
                            println("@@@ [Native] ChatSDK.Enter : $params")
                            UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
                        }
                    }
                }
                .onFailure { error ->
                    val errorJson = getErrorJson(error)
                    if (errorJson != null) {
                        valueJSONObject.put(ErrorKey, errorJson)
                    }

                    jsonObject.put(ValueKey, valueJSONObject)

                    withContext(Dispatchers.Main) {
                        val params = jsonObject.toString()
                        println("@@@ [Native] ChatSDK.Enter : $params")
                        UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
                    }
                }
        }
    }

    @JvmStatic
    fun Exit(roomId: String, identifier: String)
    {
        if(!roomSessionMap.containsKey(roomId)) {
            SendMessageRoomNotFoundError(identifier)
            return;
        }

        CoroutineScope(realmDispatcher).launch {
            val chatRoom = roomSessionMap[roomId]

            chatRoom?.roomId?.let { roomId ->
                val result = ChatSDK.exit(roomId)

                val jsonObject = JSONObject()
                jsonObject.put(IdentifierKey, identifier)

                val valueJSONObject = JSONObject()
                valueJSONObject.put(ResultKey, result.isSuccess)

                result
                    .onSuccess {
                    }
                    .onFailure { error ->
                        val errorJson = getErrorJson(error)
                        if (errorJson != null) {
                            valueJSONObject.put(ErrorKey, errorJson)
                        }
                    }

                jsonObject.put(ValueKey, valueJSONObject)

                withContext(Dispatchers.Main) {
                    val params = jsonObject.toString()
                    println("@@@ [Native] ChatSDK.Exit : $params")
                    UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
                }
            }
            roomSessionMap.remove(roomId)
        }
    }

    @JvmStatic
    fun Leave(roomId: String, identifier: String)
    {
        if(!roomSessionMap.containsKey(roomId)) {
            SendMessageRoomNotFoundError(identifier)
            return;
        }

        val roomSession = roomSessionMap[roomId];

        CoroutineScope(realmDispatcher).launch {
            roomSession?.let { room ->
                val result = ChatSDK.leave(roomId = room.roomId)

                val jsonObject = JSONObject()
                jsonObject.put(IdentifierKey, identifier)

                val valueJSONObject = JSONObject()
                valueJSONObject.put(ResultKey, result.isSuccess)

                result
                    .onSuccess {
                    }
                    .onFailure { error ->
                        val errorJson = getErrorJson(error)
                        if (errorJson != null) {
                            valueJSONObject.put(ErrorKey, errorJson)
                        }
                    }

                jsonObject.put(ValueKey, valueJSONObject)

                withContext(Dispatchers.Main) {
                    val params = jsonObject.toString()
                    println("@@@ [Native] ChatSDK.Leave : $params")
                    UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
                }
            }
        }
    }

    @JvmStatic
    fun SendMessage(roomId: String, formattedMessageJson: String, identifier: String)
    {
        if(!roomSessionMap.containsKey(roomId)) {
            SendMessageRoomNotFoundError(identifier)
            return;
        }

        val roomSession = roomSessionMap[roomId];

        val formattedMessageJsonObject = JSONObject(formattedMessageJson)
        println("@@@ [Native] ChatSDK.SendMessage formattedMessageJsonObject: $formattedMessageJsonObject")
        val formattedMessage = FormattedMessage(formattedMessageJsonObject)

        CoroutineScope(realmDispatcher).launch {
            val result = roomSession?.sendMessage(formattedMessage)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()

            result?.onSuccess {
                valueJSONObject.put(ResultKey, true)
            }
            result?.onFailure { error ->
                valueJSONObject.put(ResultKey, false)

                val errorJson = getErrorJson(error)
                if (errorJson != null) {
                    valueJSONObject.put(ErrorKey, errorJson)
                }
            }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.SendMessage : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun ResendMessage(roomId: String, chatMessageJson: String, identifier: String)
    {
        if(!roomSessionMap.containsKey(roomId)) {
            SendMessageRoomNotFoundError(identifier)
            return;
        }

        val roomSession = roomSessionMap[roomId];

        val chatMessageJsonObject = JSONObject(chatMessageJson)
        val message = ChatMessage(chatMessageJsonObject)

        CoroutineScope(realmDispatcher).launch {
            val result = roomSession?.resendMessage(message)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()

            result?.onSuccess {
                valueJSONObject.put(ResultKey, true)
            }
            result?.onFailure { error ->
                valueJSONObject.put(ResultKey, false)

                val errorJson = getErrorJson(error)
                if (errorJson != null) {
                    valueJSONObject.put(ErrorKey, errorJson)
                }
            }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.ResendMessage : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun FetchLatestMessages(roomId: String, count: Int, identifier: String)
    {
        if(!roomSessionMap.containsKey(roomId)) {
            SendMessageRoomNotFoundError(identifier)
            return;
        }

        val roomSession = roomSessionMap[roomId];

        CoroutineScope(realmDispatcher).launch {
            val result = roomSession!!.fetchLatestMessages(count)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()

            result.onSuccess {
                valueJSONObject.put(ResultKey, true)

                val sentMessages = result.getOrDefault(emptyList())
                val jsonArray = JSONArray()
                sentMessages.forEach{
                    jsonArray.put(it.toJSONObject())
                }
                valueJSONObject.put("messages", jsonArray)
            }

            result.onFailure { error ->
                valueJSONObject.put(ResultKey, false)

                val errorJson = getErrorJson(error)
                if (errorJson != null) {
                    valueJSONObject.put(ErrorKey, errorJson)
                }
            }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.FetchLatestMessages : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun FetchPreviousMessages(roomId: String, count: Int, before: Int, identifier: String)
    {
        if(!roomSessionMap.containsKey(roomId)) {
            SendMessageRoomNotFoundError(identifier)
            return;
        }

        val roomSession = roomSessionMap[roomId];

        CoroutineScope(realmDispatcher).launch {
            val result = roomSession!!.fetchPreviousMessages(count, before)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()

            result.onSuccess {
                valueJSONObject.put(ResultKey, true)

                val sentMessages = result.getOrDefault(emptyList())
                val jsonArray = JSONArray()
                sentMessages.forEach{
                    jsonArray.put(it.toJSONObject())
                }
                valueJSONObject.put("messages", jsonArray)
            }

            result.onFailure { error ->
                valueJSONObject.put(ResultKey, false)

                val errorJson = getErrorJson(error)
                if (errorJson != null) {
                    valueJSONObject.put(ErrorKey, errorJson)
                }
            }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.FetchPreviousMessages : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun FetchNextMessages(roomId: String, count: Int, since: Int, identifier: String)
    {
        if(!roomSessionMap.containsKey(roomId)) {
            SendMessageRoomNotFoundError(identifier)
            return;
        }

        val roomSession = roomSessionMap[roomId];

        CoroutineScope(realmDispatcher).launch {
            val result = roomSession!!.fetchNextMessages(count, since)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()

            result.onSuccess {
                valueJSONObject.put(ResultKey, true)

                val sentMessages = result.getOrDefault(emptyList())
                val jsonArray = JSONArray()
                sentMessages.forEach{
                    jsonArray.put(it.toJSONObject())
                }
                valueJSONObject.put("messages", jsonArray)
            }

            result.onFailure { error ->
                valueJSONObject.put(ResultKey, false)

                val errorJson = getErrorJson(error)
                if (errorJson != null) {
                    valueJSONObject.put(ErrorKey, errorJson)
                }
            }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.FetchNextMessages : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun FetchUnsentMessages(roomId: String, identifier: String)
    {
        if(!roomSessionMap.containsKey(roomId)) {
            SendMessageRoomNotFoundError(identifier)
            return;
        }

        val roomSession = roomSessionMap[roomId];

        CoroutineScope(realmDispatcher).launch {
            val result = roomSession!!.fetchUnsentMessages()

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()

            result.onSuccess {
                valueJSONObject.put(ResultKey, true)

                val sentMessages = result.getOrDefault(emptyList())
                val jsonArray = JSONArray()
                sentMessages.forEach{
                    jsonArray.put(it.toJSONObject())
                }
                valueJSONObject.put("messages", jsonArray)
            }

            result.onFailure { error ->
                valueJSONObject.put(ResultKey, false)

                val errorJson = getErrorJson(error)
                if (errorJson != null) {
                    valueJSONObject.put(ErrorKey, errorJson)
                }
            }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.FetchUnsentMessages : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun DeleteUnsentMessage(roomId: String, messageId: String, identifier: String)
    {
        if(!roomSessionMap.containsKey(roomId)) {
            SendMessageRoomNotFoundError(identifier)
            return;
        }

        val roomSession = roomSessionMap[roomId];

        CoroutineScope(realmDispatcher).launch {
            val result = roomSession!!.deleteUnsentMessage(messageId)

            val jsonObject = JSONObject()
            jsonObject.put(IdentifierKey, identifier)

            val valueJSONObject = JSONObject()
            valueJSONObject.put(ResultKey, result.isSuccess)

            result.onSuccess {
            }

            result.onFailure { error ->
                val errorJson = getErrorJson(error)
                if (errorJson != null) {
                    valueJSONObject.put(ErrorKey, errorJson)
                }
            }

            jsonObject.put(ValueKey, valueJSONObject)

            withContext(Dispatchers.Main) {
                val params = jsonObject.toString()
                println("@@@ [Native] ChatSDK.DeleteUnsentMessage : $params")
                UnityPlayer.UnitySendMessage(objectName, CallMessage, params)
            }
        }
    }

    @JvmStatic
    fun RegisterChatRoomEvent(roomId: String, identifier: String)
    {
        val listenerIdentifier = identifier;

        if(listenerIdentifier.isEmpty()) {
            return
        }

        if(registerChatRoomEventMap.containsKey(roomId))
        {
            if(registerChatRoomEventMap[roomId] != null)
                return;
        }

        println("@@@ [Native] call ChatSDK.RegisterChatRoomEvent")
        registerChatRoomEventMap[roomId] = CoroutineScope(Dispatchers.Default).launch {
            if(roomSessionMap.containsKey(roomId)) {
                val chatRoom = roomSessionMap[roomId];
                chatRoom!!.chatRoomEvent.collect { event ->
                    when (event) {
                        is ChatRoomEvent.MessageReceived -> {
                            val newMessage = event.message

                            val jsonObject = JSONObject()
                            jsonObject.put(IdentifierKey, listenerIdentifier)

                            val valueJSONObject = JSONObject()
                            valueJSONObject.put("MessageReceived", newMessage.toJSONObject())

                            jsonObject.put(ValueKey, valueJSONObject)

                            withContext(Dispatchers.Main) {
                                val params = jsonObject.toString()
                                println("@@@ [Native] ChatSDK.MessageReceived RoomId: ${roomId} : $params")
                                UnityPlayer.UnitySendMessage(objectName, NotifyMessage, params)
                            }
                        }

                        is ChatRoomEvent.RoomUpdated -> {
                            val room = event.room

                            val jsonObject = JSONObject()
                            jsonObject.put(IdentifierKey, listenerIdentifier)

                            val valueJSONObject = JSONObject()
                            valueJSONObject.put("RoomUpdated", room.toJSONObject())

                            jsonObject.put(ValueKey, valueJSONObject)

                            withContext(Dispatchers.Main) {
                                val params = jsonObject.toString()
                                println("@@@ [Native] ChatSDK.RoomUpdated RoomId: ${roomId} : $params")
                                UnityPlayer.UnitySendMessage(objectName, NotifyMessage, params)
                            }
                        }
                    }
                }
            }
        }
    }

    @JvmStatic
    fun UnregisterChatRoomEvent(roomId: String)
    {
        if(registerChatRoomEventMap.containsKey(roomId))
        {
            println("@@@ [Native] ChatSDK.UnregisterChatRoomEvent")
            registerChatRoomEventMap[roomId]!!.cancel()
            registerChatRoomEventMap[roomId] = null
            registerChatRoomEventMap.remove(roomId)
        }
    }

    @JvmStatic
    fun RegisterChatEvent(identifier: String)
    {
        val listenerIdentifier = identifier;

        if(listenerIdentifier.isEmpty()) {
            return
        }

        if(registerChatEvent != null) {
            return
        }

        println("@@@ [Native] call ChatSDK.RegisterChatEvent")
        registerChatEvent = CoroutineScope(Dispatchers.Default).launch {
            ChatSDK.chatEvent.collect { event ->
                val jsonObject = JSONObject()
                jsonObject.put(IdentifierKey, listenerIdentifier)

                val valueJSONObject = JSONObject()

                when (event) {
                    is ChatEvent.RoomCreated -> {
                        valueJSONObject.put("type", 0)
                        valueJSONObject.put("room", event.room.toJSONObject())
                    }

                    is ChatEvent.RoomInvited -> {
                        valueJSONObject.put("type", 1)
                        valueJSONObject.put("room", event.room.toJSONObject())
                    }

                    is ChatEvent.RoomUpdated -> {
                        valueJSONObject.put("type", 2)
                        valueJSONObject.put("room", event.room.toJSONObject())
                    }

                    is ChatEvent.RoomDeleted -> {
                        valueJSONObject.put("type", 3)
                        valueJSONObject.put("roomId", event.roomId)
                    }
                }

                jsonObject.put(ValueKey, valueJSONObject)

                withContext(Dispatchers.Main) {
                    val params = jsonObject.toString()
                    println("@@@ [Native] ChatSDK.ChatEvent : $params")
                    UnityPlayer.UnitySendMessage(objectName, NotifyMessage, params)
                }
            }
        }
    }

    @JvmStatic
    fun UnregisterChatEvent()
    {
        if(registerChatEvent != null) {
            println("@@@ [Native] ChatSDK.UnregisterChatEvent")
            registerChatEvent!!.cancel()
            registerChatEvent = null
        }
    }

    @JvmStatic
    fun RegisterConnectionStatus(identifier: String)
    {
        val listenerIdentifier = identifier;

        if(listenerIdentifier.isEmpty()) {
            return
        }

        if(registerConnectionStatus != null) {
            return
        }

        println("@@@ [Native] call ChatSDK.RegisterConnectionStatus")
        registerConnectionStatus = CoroutineScope(Dispatchers.Default).launch {
            ChatSDK.connectionStatus.collect { event ->
                val jsonObject = JSONObject()
                jsonObject.put(IdentifierKey, listenerIdentifier)
                jsonObject.put(ValueKey, event.toJSONObject())

                withContext(Dispatchers.Main) {
                    val params = jsonObject.toString()
                    println("@@@ [Native] ChatSDK.RegisterConnectionStatus : $params")
                    UnityPlayer.UnitySendMessage(objectName, NotifyMessage, params)
                }
            }
        }
    }

    @JvmStatic
    fun UnregisterConnectionStatus()
    {
        if(registerConnectionStatus != null) {
            println("@@@ [Native] ChatSDK.UnregisterConnectionStatus")
            registerConnectionStatus!!.cancel()
            registerConnectionStatus = null
        }
    }

    @JvmStatic
    fun GetConnectionStatus(): String
    {
        val status = ChatSDK.connectionStatus.value
        return status.toJSONObject().toString()
    }

    @JvmStatic
    fun UpdateCredential(userId: String, token: String)
    {
        CoroutineScope(realmDispatcher).launch {
            val credential = ChatCredential(userId, token)
            credential?.let {
                println("@@@ [Native] UpdateCredential $credential")
                ChatSDK.updateCredential(credential!!)
            }
        }
    }

    fun getErrorJson(throwable: Throwable): JSONObject? {
        if (throwable is ChatError) {
            val type = when (throwable) {
                is ChatError.AuthenticationDenied -> "AuthenticationDenied"
                is ChatError.BanWords -> "BanWords"
                is ChatError.ConnectionFailed -> "ConnectionFailed"
                is ChatError.NotInitialized -> "NotInitialized"
                is ChatError.NotParticipated -> "NotParticipated"
                is ChatError.PermissionDenied -> "PermissionDenied"
                is ChatError.RoomNotFound -> "RoomNotFound"
                is ChatError.Server -> "Server"
                is ChatError.NotConnected -> "NotConnected"
            }
            val message = throwable.message ?: ""
            val json = JSONObject()
            json.put("type", type)
            json.put("message", message)
            return json
        }
        return null
    }

    fun SendMessageRoomNotFoundError(identifier: String)
    {
        val jsonObject = JSONObject()
        jsonObject.put(IdentifierKey, identifier)

        val valueJSONObject = JSONObject()
        valueJSONObject.put(ResultKey, false)

        valueJSONObject.put(ErrorKey, roomNotFoundErrorJson())

        jsonObject.put(ValueKey, valueJSONObject)

        val params = jsonObject.toString()
        println("@@@ [Native] ChatSDK.SendMessageRoomNotFoundError : $params")
        UnityPlayer.UnitySendMessage(objectName, CallMessage, params)

    }

    fun roomNotFoundErrorJson(): JSONObject?
    {
        val json = JSONObject()
        json.put("type", "RoomNotFound")
        json.put("message", "Chat room not found")
        return json
    }
}